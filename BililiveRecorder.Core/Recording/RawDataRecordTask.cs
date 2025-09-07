using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Event;
using BililiveRecorder.Common.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BililiveRecorder.Core.Recording;

internal class RawDataRecordTask : RecordTaskBase
{
    private RecordFileOpeningEventArgs? _fileOpeningEventArgs;

    public RawDataRecordTask(IRoom room,
        ILogger logger,
        IServiceProvider serviceProvider,
        UserScriptRunner userScriptRunner,
        IDispatchProvider dispatchProvider)
        : base(room: room,
            logger: logger.ForContext<RawDataRecordTask>().ForContext(LoggingContext.RoomId, room.RoomConfig.RoomId)!,
            apiClient: serviceProvider.GetRequiredKeyedService<IApiClient>(room.RoomConfig.Platform),
            userScriptRunner: userScriptRunner,
            dispatchProvider: dispatchProvider)
    {
    }

    public override void SplitOutput()
    {
    }

    protected override void StartRecordingLoop(Stream stream)
    {
        var (fullPath, relativePath) = CreateFileName();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        }
        catch (Exception)
        {
        }

        _fileOpeningEventArgs = new RecordFileOpeningEventArgs(room)
        {
            SessionId = SessionId,
            FullPath = fullPath,
            RelativePath = relativePath,
            FileOpenTime = DateTimeOffset.Now,
        };
        OnRecordFileOpening(_fileOpeningEventArgs);

        logger.Information("新建录制文件 {Path}", fullPath);

        var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);

        _ = Task.Run(async () => await WriteStreamToFileAsync(stream, file).ConfigureAwait(false));
    }

    private async Task WriteStreamToFileAsync(Stream stream, FileStream file)
    {
        try
        {
            var buffer = new byte[1024 * 8];
            timer.Start();

            while (!ct.IsCancellationRequested)
            {
#if NET6_0_OR_GREATER
                var bytesRead = await stream.ReadAsync(buffer, ct).ConfigureAwait(false);
#else
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, this.ct).ConfigureAwait(false);
#endif
                if (bytesRead == 0)
                    break;

                Interlocked.Add(ref ioNetworkDownloadedBytes, bytesRead);

                ioDiskStopwatch.Restart();

#if NET6_0_OR_GREATER
                await file.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
#else
                    await file.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
#endif

                ioDiskStopwatch.Stop();

                lock (ioDiskStatsLock)
                {
                    ioDiskWriteDuration += ioDiskStopwatch.Elapsed;
                    ioDiskWrittenBytes += bytesRead;
                }

                ioDiskStopwatch.Reset();
            }
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
            timer.Stop();
            RequestStop();

            RecordFileClosedEventArgs? recordFileClosedEvent;
            if (_fileOpeningEventArgs is { } openingEventArgs)
                recordFileClosedEvent = new RecordFileClosedEventArgs(room)
                {
                    SessionId = SessionId,
                    FullPath = openingEventArgs.FullPath,
                    RelativePath = openingEventArgs.RelativePath,
                    FileOpenTime = openingEventArgs.FileOpenTime,
                    FileCloseTime = DateTimeOffset.Now,
                    Duration = 0,
                    FileSize = file.Length,
                };
            else
                recordFileClosedEvent = null;

            try
            {
#if NET6_0_OR_GREATER
                await file.DisposeAsync();
#else
                    file.Dispose();
#endif
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "关闭文件时发生错误");
            }

            try
            {
#if NET6_0_OR_GREATER
                await stream.DisposeAsync();
#else
                    stream.Dispose();
#endif
            }
            catch (Exception)
            {
            }

            try
            {
                if (recordFileClosedEvent is not null)
                    OnRecordFileClosed(recordFileClosedEvent);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error calling OnRecordFileClosed");
            }

            OnRecordSessionEnded(EventArgs.Empty);

            logger.Information("录制结束");
        }
    }
}
