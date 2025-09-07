using System.IO.Pipelines;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Event;
using BililiveRecorder.Common.Scripting;
using BililiveRecorder.Core.ProcessingRules;
using BililiveRecorder.Flv;
using BililiveRecorder.Flv.Amf;
using BililiveRecorder.Flv.Parser;
using BililiveRecorder.Flv.Pipeline;
using BililiveRecorder.Flv.Pipeline.Actions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BililiveRecorder.Core.Recording;

internal class StandardRecordTask : RecordTaskBase
{
    private readonly IFlvTagReaderFactory flvTagReaderFactory;
    private readonly ITagGroupReaderFactory tagGroupReaderFactory;
    private readonly IFlvProcessingContextWriterFactory writerFactory;
    private readonly ProcessingDelegate pipeline;

    private readonly IFlvWriterTargetProvider targetProvider;
    private readonly StatsRule statsRule;
    private readonly SplitRule splitFileRule;

    private readonly FlvProcessingContext context = new();
    private readonly IDictionary<object, object?> session = new Dictionary<object, object?>();

    private ITagGroupReader? reader;
    private IFlvProcessingContextWriter? writer;

    public StandardRecordTask(IRoom room,
        ILogger logger,
        IProcessingPipelineBuilder builder,
        IServiceProvider serviceProvider,
        IFlvTagReaderFactory flvTagReaderFactory,
        ITagGroupReaderFactory tagGroupReaderFactory,
        IFlvProcessingContextWriterFactory writerFactory,
        UserScriptRunner userScriptRunner,
        IDispatchProvider dispatchProvider)
        : base(room: room,
            logger: logger.ForContext<StandardRecordTask>().ForContext(LoggingContext.RoomId, room.RoomConfig.RoomId)!,
            apiClient: serviceProvider.GetRequiredKeyedService<IApiClient>(room.RoomConfig.Platform),
            userScriptRunner: userScriptRunner,
            dispatchProvider: dispatchProvider)
    {
        this.flvTagReaderFactory = flvTagReaderFactory ?? throw new ArgumentNullException(nameof(flvTagReaderFactory));
        this.tagGroupReaderFactory = tagGroupReaderFactory ?? throw new ArgumentNullException(nameof(tagGroupReaderFactory));
        this.writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));

        ArgumentNullException.ThrowIfNull(builder);

        statsRule = new StatsRule();
        splitFileRule = new SplitRule();

        statsRule.StatsUpdated += StatsRule_StatsUpdated;

        pipeline = builder
            .ConfigureServices(services => services.AddSingleton(new ProcessingPipelineSettings
            {
                SplitOnScriptTag = room.RoomConfig.FlvProcessorSplitOnScriptTag,
                DisableSplitOnH264AnnexB = room.RoomConfig.FlvProcessorDisableSplitOnH264AnnexB,
            }))
            .AddRule(statsRule)
            .AddRule(splitFileRule)
            .AddDefaultRules()
            .AddRemoveFillerDataRule()
            .Build();

        targetProvider = new WriterTargetProvider(this, paths =>
        {
            this.logger.ForContext(LoggingContext.RoomId, this.room.RoomConfig.RoomId).Information("新建录制文件 {Path}", paths.fullPath);

            var e = new RecordFileOpeningEventArgs(this.room)
            {
                SessionId = SessionId,
                FullPath = paths.fullPath,
                RelativePath = paths.relativePath,
                FileOpenTime = DateTimeOffset.Now,
            };
            OnRecordFileOpening(e);
            return e;
        });
    }

    public override void SplitOutput() => splitFileRule.SetSplitBeforeFlag();

    protected override void StartRecordingLoop(Stream stream)
    {
        var pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));

        reader = tagGroupReaderFactory.CreateTagGroupReader(flvTagReaderFactory.CreateFlvTagReader(pipe.Reader));

        writer = writerFactory.CreateWriter(targetProvider);
        writer.BeforeScriptTagWrite = Writer_BeforeScriptTagWrite;
        writer.FileClosed += (sender, e) =>
        {
            var openingEventArgs = (RecordFileOpeningEventArgs)e.State!;
            OnRecordFileClosed(new RecordFileClosedEventArgs(room)
            {
                SessionId = SessionId,
                FullPath = openingEventArgs.FullPath,
                RelativePath = openingEventArgs.RelativePath,
                FileOpenTime = openingEventArgs.FileOpenTime,
                FileCloseTime = DateTimeOffset.Now,
                Duration = e.Duration,
                FileSize = e.FileSize,
            });
        };

        _ = Task.Run(async () => await FillPipeAsync(stream, pipe.Writer).ConfigureAwait(false));

        _ = Task.Run(RecordingLoopAsync);
    }

    private async Task FillPipeAsync(Stream stream, PipeWriter writer)
    {
        const int minimumBufferSize = 1024;
        timer.Start();

        Exception? exception = null;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    var bytesRead = await stream.ReadAsync(memory, ct).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;
                    writer.Advance(bytesRead);
                    _ = Interlocked.Add(ref ioNetworkDownloadedBytes, bytesRead);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    break;
                }

                var result = await writer.FlushAsync(ct).ConfigureAwait(false);
                if (result.IsCompleted)
                    break;
            }
        }
        finally
        {
            timer.Stop();
#if NET6_0_OR_GREATER
            await stream.DisposeAsync().ConfigureAwait(false);
#else
                stream.Dispose();
#endif
            await writer.CompleteAsync(exception).ConfigureAwait(false);
        }
    }

    private async Task RecordingLoopAsync()
    {
        try
        {
            if (reader is null) return;
            if (writer is null) return;

            while (!ct.IsCancellationRequested)
            {
                var group = await reader.ReadGroupAsync(ct).ConfigureAwait(false);

                if (group is null)
                    break;

                context.Reset(group, session);

                pipeline(context);

                if (context.Comments.Count > 0)
                    logger.Debug("修复逻辑输出 {@Comments}", context.Comments);

                ioDiskStopwatch.Restart();
                var bytesWritten = await writer.WriteAsync(context).ConfigureAwait(false);
                ioDiskStopwatch.Stop();

                lock (ioDiskStatsLock)
                {
                    ioDiskWriteDuration += ioDiskStopwatch.Elapsed;
                    ioDiskWrittenBytes += bytesWritten;
                }

                ioDiskStopwatch.Reset();

                if (context.Actions.FirstOrDefault(x => x is PipelineDisconnectAction) is PipelineDisconnectAction disconnectAction)
                {
                    logger.Information("修复系统断开录制：{Reason}", disconnectAction.Reason);
                    break;
                }
            }
        }
        catch (UnsupportedCodecException ex)
        {
            // 直播流不是 H.264
            logger.Warning(ex, "不支持此直播流的视频编码格式（只支持 H.264），下次录制会尝试使用原始模式录制");
            room.MarkNextRecordShouldUseRawMode();
        }
        catch (OperationCanceledException ex)
        {
            logger.Debug(ex, "录制被取消");
        }
        catch (IOException ex)
        {
            logger.Warning(ex, "录制时发生IO错误");
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "录制时发生了错误");
        }
        finally
        {
            reader?.Dispose();
            reader = null;
            writer?.Dispose();
            writer = null;
            RequestStop();

            OnRecordSessionEnded(EventArgs.Empty);

            logger.Information("录制结束");
        }
    }

    private void Writer_BeforeScriptTagWrite(ScriptTagBody scriptTagBody)
    {
        if (!room.RoomConfig.FlvWriteMetadata)
            return;

        if (scriptTagBody.Values is [_, ScriptDataEcmaArray value])
        {
            var now = DateTimeOffset.Now;
            value["Title"] = (ScriptDataString)room.Title;
            value["Artist"] = (ScriptDataString)$"{room.Name} ({room.RoomConfig.RoomId})";
            value["Comment"] = (ScriptDataString)
                ($"mikufans直播间 {room.RoomConfig.RoomId} 的直播录像\n" +
                 $"主播名: {room.Name}\n" +
                 $"直播标题: {room.Title}\n" +
                 $"直播分区: {room.AreaNameParent}·{room.AreaNameChild}\n" +
                 $"录制时间: {now:O}\n" +
                 $"直播服务器:\n" +
                 $"{streamHostFull}\n" +
                 $"\n" +
                 $"使用 mikufans录播姬 录制 https://rec.danmuji.org\n" +
                 $"录播姬版本: {GitVersionInformation.FullSemVer}");
            value["BililiveRecorder"] = new ScriptDataEcmaArray
            {
                ["RecordedBy"] = (ScriptDataString)"BililiveRecorder mikufans录播姬",
                ["RecordedFrom"] = (ScriptDataString)(streamHost ?? string.Empty),
                ["StreamServers"] = (ScriptDataString)(streamHostFull ?? string.Empty),
                ["RecorderVersion"] = (ScriptDataString)GitVersionInformation.InformationalVersion,
                ["StartTime"] = (ScriptDataDate)now,
                ["RoomId"] = (ScriptDataString)room.RoomConfig.RoomId.ToString(),
                ["ShortId"] = (ScriptDataString)room.ShortId.ToString(),
                ["Name"] = (ScriptDataString)room.Name,
                ["StreamTitle"] = (ScriptDataString)room.Title,
                ["AreaNameParent"] = (ScriptDataString)room.AreaNameParent,
                ["AreaNameChild"] = (ScriptDataString)room.AreaNameChild,
            };
        }
    }

    private void StatsRule_StatsUpdated(object? sender, RecordingStatsEventArgs e)
    {
        switch (room.RoomConfig.CuttingMode)
        {
            case CuttingMode.ByTime:
                if (e.FileMaxTimestamp > room.RoomConfig.CuttingNumber * 60u * 1000u)
                    splitFileRule.SetSplitBeforeFlag();
                break;
            case CuttingMode.BySize:
                if ((e.CurrentFileSize + (e.OutputVideoBytes * 1.1) + e.OutputAudioBytes) / (1024d * 1024d) > room.RoomConfig.CuttingNumber)
                    splitFileRule.SetSplitBeforeFlag();
                break;
        }

        OnRecordingStats(e);
    }

    internal class WriterTargetProvider : IFlvWriterTargetProvider
    {
        private readonly StandardRecordTask task;
        private readonly Func<(string fullPath, string relativePath), object> OnNewFile;

        private string last_path = string.Empty;

        public WriterTargetProvider(StandardRecordTask task, Func<(string fullPath, string relativePath), object> onNewFile)
        {
            this.task = task ?? throw new ArgumentNullException(nameof(task));
            OnNewFile = onNewFile ?? throw new ArgumentNullException(nameof(onNewFile));
        }

        public (Stream stream, object? state) CreateOutputStream()
        {
            var paths = task.CreateFileName();

            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(paths.fullPath)!);
            }
            catch (Exception)
            {
            }

            last_path = paths.fullPath;
            var state = OnNewFile(paths);

            var stream = new FileStream(paths.fullPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            return (stream, state);
        }

        public Stream CreateAccompanyingTextLogStream()
        {
            var path = string.IsNullOrWhiteSpace(last_path)
                ? Path.ChangeExtension(task.CreateFileName().fullPath, "txt")
                : Path.ChangeExtension(last_path, "txt");

            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            catch (Exception)
            {
            }

            var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            return stream;
        }
    }
}
