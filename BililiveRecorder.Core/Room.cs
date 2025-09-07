using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Common.Danmaku;
using BililiveRecorder.Common.Event;
using BililiveRecorder.Common.Scripting;
using BililiveRecorder.Core.Api;
using BililiveRecorder.Core.Recording;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Serilog;
using Serilog.Events;
using Timer = System.Timers.Timer;

namespace BililiveRecorder.Core;

internal class Room : IRoom
{
    private const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
    private const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

    private readonly object recordStartLock = new object();
    private readonly SemaphoreSlim recordRetryDelaySemaphoreSlim = new(1, 1);
    private readonly Timer timer;

    private readonly IServiceScope scope;
    private readonly ILogger loggerWithoutContext;
    private readonly IDanmakuClient danmakuClient;
    private readonly IApiClient apiClient;
    private readonly IBasicDanmakuWriter basicDanmakuWriter;
    private readonly IRecordTaskFactory recordTaskFactory;
    private readonly UserScriptRunner userScriptRunner;
    private readonly CancellationTokenSource cts;
    private readonly CancellationToken ct;
    private readonly IDispatchProvider dispatchProvider;

    private ILogger logger;
    private bool disposedValue;

    private int shortId;
    private string name = string.Empty;
    private long uid;
    private string title = string.Empty;
    private string areaNameParent = string.Empty;
    private string areaNameChild = string.Empty;
    private bool danmakuConnected;
    private bool streaming;
    private bool autoRecordForThisSession = true;
    private bool nextRecordShouldUseRawMode = false;

    private IRecordTask? recordTask;
    private DateTimeOffset danmakuClientConnectTime;
    private readonly ManualResetEventSlim danmakuConnectHoldOff = new();

    private static readonly TimeSpan danmakuClientReconnectNoDelay = TimeSpan.FromMinutes(1);
    private static readonly HttpClient coverDownloadHttpClient = new();

    static Room()
    {
        coverDownloadHttpClient.Timeout = TimeSpan.FromSeconds(10);
        coverDownloadHttpClient.DefaultRequestHeaders.UserAgent.Clear();
    }

    public Room(IServiceScope scope, RoomConfig roomConfig, int initDelayFactor, ILogger logger,
        IRecordTaskFactory recordTaskFactory, UserScriptRunner userScriptRunner,
        IDispatchProvider dispatchProvider)
    {
        this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
        RoomConfig = roomConfig ?? throw new ArgumentNullException(nameof(roomConfig));
        loggerWithoutContext = logger.ForContext<Room>() ?? throw new ArgumentNullException(nameof(logger));
        this.logger = loggerWithoutContext.ForContext(LoggingContext.RoomId, RoomConfig.RoomId);
        danmakuClient = scope.ServiceProvider.GetRequiredKeyedService<IDanmakuClient>(roomConfig.Platform) ??
                        throw new ArgumentNullException(nameof(danmakuClient));
        apiClient = scope.ServiceProvider.GetRequiredKeyedService<IApiClient>(roomConfig.Platform) ??
                    throw new ArgumentNullException(nameof(apiClient));
        basicDanmakuWriter = scope.ServiceProvider.GetRequiredKeyedService<IBasicDanmakuWriter>(roomConfig.Platform) ??
                             throw new ArgumentNullException(nameof(basicDanmakuWriter));
        this.recordTaskFactory = recordTaskFactory ?? throw new ArgumentNullException(nameof(recordTaskFactory));
        this.userScriptRunner = userScriptRunner ?? throw new ArgumentNullException(nameof(userScriptRunner));
        this.dispatchProvider = dispatchProvider ?? throw new ArgumentNullException(nameof(dispatchProvider));

        timer = new Timer(RoomConfig.TimingCheckInterval * 1000d);
        cts = new CancellationTokenSource();
        ct = cts.Token;

        PropertyChanged += Room_PropertyChanged;
        RoomConfig.PropertyChanged += RoomConfig_PropertyChanged;

        timer.Elapsed += Timer_Elapsed;

        danmakuClient.StatusChanged += DanmakuClient_StatusChanged;
        danmakuClient.DanmakuReceived += DanmakuClient_DanmakuReceived;
        danmakuClient.BeforeHandshake = DanmakuClient_BeforeHandshake;

        _ = Task.Run(async () =>
        {
            await Task.Delay(1500 + (initDelayFactor * 500));
            timer.Start();
            await RefreshRoomInfoAsync();
        });
    }

    public int ShortId
    {
        get => shortId;
        private set => SetField(ref shortId, value);
    }

    public string Name
    {
        get => name;
        private set => SetField(ref name, value);
    }

    public long Uid
    {
        get => uid;
        private set => SetField(ref uid, value);
    }

    public string Title
    {
        get => title;
        private set => SetField(ref title, value);
    }

    public string AreaNameParent
    {
        get => areaNameParent;
        private set => SetField(ref areaNameParent, value);
    }

    public string AreaNameChild
    {
        get => areaNameChild;
        private set => SetField(ref areaNameChild, value);
    }

    public JObject? RawApiJsonData { get; private set; }

    public bool Streaming
    {
        get => streaming;
        private set => SetField(ref streaming, value);
    }

    public bool AutoRecordForThisSession
    {
        get => autoRecordForThisSession;
        private set => SetField(ref autoRecordForThisSession, value);
    }

    public bool DanmakuConnected
    {
        get => danmakuConnected;
        private set => SetField(ref danmakuConnected, value);
    }

    public bool Recording => recordTask != null;

    public RoomConfig RoomConfig { get; }
    public RoomStats Stats { get; } = new();

    public Guid ObjectId { get; } = Guid.NewGuid();

    public event EventHandler<RecordSessionStartedEventArgs>? RecordSessionStarted;
    public event EventHandler<RecordSessionEndedEventArgs>? RecordSessionEnded;
    public event EventHandler<RecordFileOpeningEventArgs>? RecordFileOpening;
    public event EventHandler<RecordFileClosedEventArgs>? RecordFileClosed;
    public event EventHandler<IOStatsEventArgs>? IOStats;
    public event EventHandler<RecordingStatsEventArgs>? RecordingStats;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SplitOutput()
    {
        if (disposedValue)
            return;

        lock (recordStartLock)
        {
            recordTask?.SplitOutput();
        }
    }

    public void StartRecord()
    {
        if (disposedValue)
            return;

        lock (recordStartLock)
        {
            AutoRecordForThisSession = true;

            _ = Task.Run(() =>
            {
                try
                {
                    // 手动触发录制，启动录制前再刷新一次房间信息
                    CreateAndStartNewRecordTask(skipFetchRoomInfo: false);
                }
                catch (Exception ex)
                {
                    logger.Write(
                        ex is ExecutionRejectedException ? LogEventLevel.Verbose : LogEventLevel.Warning, ex,
                        "尝试开始录制时出错");
                }
            });
        }
    }

    public void StopRecord()
    {
        if (disposedValue)
            return;

        lock (recordStartLock)
        {
            AutoRecordForThisSession = false;

            if (recordTask == null)
                return;

            recordTask.RequestStop();
        }
    }

    public async Task RefreshRoomInfoAsync()
    {
        if (disposedValue)
            return;

        try
        {
            // 如果直播状态从 false 改成 true，Room_PropertyChanged 会触发录制
            await FetchRoomInfoAsync().ConfigureAwait(false);

            StartDamakuConnection(delay: false);
        }
        catch (Exception ex)
        {
            logger.Write(ex is ExecutionRejectedException ? LogEventLevel.Verbose : LogEventLevel.Warning, ex,
                "刷新房间信息时出错");
        }
    }

    #region Recording

    /// <exception cref="Exception"/>
    private async Task FetchRoomInfoAsync()
    {
        if (disposedValue)
            return;
        var room = await apiClient.GetRoomInfoAsync(RoomConfig.RoomId).ConfigureAwait(true);
        if (room != null)
        {
            logger.Debug("拉取房间信息成功: {@room}", room);

            dispatchProvider.DispatchToUiThread(() =>
            {
                RoomConfig.RoomId = room.Room.RoomId;
                ShortId = room.Room.ShortId;
                Uid = room.Room.Uid;
                Title = room.Room.Title;
                AreaNameParent = room.Room.ParentAreaName;
                AreaNameChild = room.Room.AreaName;
                Streaming = room.Room.LiveStatus == 1;

                Name = room.User.Name;

                RawApiJsonData = room.RawApiJsonData;

                // allow danmaku client to connect
                danmakuConnectHoldOff.Set();
            });
        }
    }

    public void MarkNextRecordShouldUseRawMode()
    {
        nextRecordShouldUseRawMode = true;
    }

    private static readonly TimeSpan TitleRegexMatchTimeout = TimeSpan.FromSeconds(0.5);

    /// <exception cref="ArgumentException" />
    /// <exception cref="RegexMatchTimeoutException" />
    private bool DoesTitleAllowRecord()
    {
        // 按新行分割的正则表达式
        var patterns =
            RoomConfig.TitleFilterPatterns?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (patterns is null || patterns.Length == 0)
            return true;

        return patterns.All(pattern => !Regex.IsMatch(input: Title, pattern: pattern, options: RegexOptions.None,
            matchTimeout: TitleRegexMatchTimeout));
    }

    ///
    private void CreateAndStartNewRecordTask(bool skipFetchRoomInfo = false)
    {
        lock (recordStartLock)
        {
            if (disposedValue)
                return;

            if (!Streaming)
                return;

            if (recordTask != null)
                return;

            try
            {
                if (!DoesTitleAllowRecord())
                {
                    logger.Information("标题匹配到跳过录制设置中的规则，不录制");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "检查标题是否匹配跳过录制正则表达式时出错");
            }

            var task = recordTaskFactory.CreateRecordTask(this,
                nextRecordShouldUseRawMode ? RecordMode.RawData : null);
            nextRecordShouldUseRawMode = false;

            task.IOStats += RecordTask_IOStats;
            task.RecordingStats += RecordTask_RecordingStats;
            task.RecordFileOpening += RecordTask_RecordFileOpening;
            task.RecordFileClosed += RecordTask_RecordFileClosed;
            task.RecordSessionEnded += RecordTask_RecordSessionEnded;
            recordTask = task;
            Stats.Reset();
            OnPropertyChanged(nameof(Recording));

            _ = Task.Run(async () =>
            {
                try
                {
                    if (!skipFetchRoomInfo)
                        await FetchRoomInfoAsync();

                    await recordTask.StartAsync();
                }
                catch (NoMatchingQnValueException)
                {
                    recordTask = null;
                    OnPropertyChanged(nameof(Recording));

                    // 无匹配的画质，重试录制之前等待更长时间
                    _ = Task.Run(() =>
                        RestartAfterRecordTaskFailedAsync(RestartRecordingReason.NoMatchingQnValue));

                    return;
                }
                catch (Exception ex)
                {
                    logger.Write(
                        ex is ExecutionRejectedException ? LogEventLevel.Verbose : LogEventLevel.Warning, ex,
                        "启动录制出错");

                    recordTask = null;
                    OnPropertyChanged(nameof(Recording));

                    if (ex is IOException ioex && (ioex.HResult == HR_ERROR_DISK_FULL ||
                                                   ioex.HResult == HR_ERROR_HANDLE_DISK_FULL))
                    {
                        logger.Warning("因为硬盘空间已满，本次不再自动重试启动录制。");
                        return;
                    }
                    else if (ex is BilibiliApiResponseCodeNotZeroException notzero && notzero.Code == 19002005)
                    {
                        // 房间已加密
                        logger.Warning("房间已加密，无密码获取不到直播流，本次不再自动重试启动录制。");
                        return;
                    }
                    else
                    {
                        // 请求直播流出错时的重试逻辑
                        _ = Task.Run(() => RestartAfterRecordTaskFailedAsync(RestartRecordingReason.GenericRetry), ct);
                        return;
                    }
                }

                RecordSessionStarted?.Invoke(this, new RecordSessionStartedEventArgs(this)
                {
                    SessionId = recordTask.SessionId
                });
            });
        }
    }

    ///
    private async Task RestartAfterRecordTaskFailedAsync(RestartRecordingReason restartRecordingReason)
    {
        if (disposedValue)
            return;
        if (!Streaming || !AutoRecordForThisSession)
            return;

        try
        {
            if (!await recordRetryDelaySemaphoreSlim.WaitAsync(0).ConfigureAwait(false))
                return;

            try
            {
                var delay = restartRecordingReason switch
                {
                    RestartRecordingReason.GenericRetry => RoomConfig.TimingStreamRetry,
                    RestartRecordingReason.NoMatchingQnValue => RoomConfig.TimingStreamRetryNoQn * 1000,
                    _ => throw new InvalidOperationException()
                };
                await Task.Delay((int)delay, ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // 房间已经被删除
                return;
            }
            finally
            {
                _ = recordRetryDelaySemaphoreSlim.Release();
            }

            // 如果状态是非直播中，跳过重试尝试。当状态切换到直播中时会开始新的录制任务。
            if (!Streaming || !AutoRecordForThisSession)
                return;

            // 启动录制时更新房间信息
            if (Streaming && AutoRecordForThisSession)
                CreateAndStartNewRecordTask(skipFetchRoomInfo: false);
        }
        catch (Exception ex)
        {
            logger.Write(ex is ExecutionRejectedException ? LogEventLevel.Verbose : LogEventLevel.Warning, ex,
                "重试开始录制时出错");
            _ = Task.Run(() => RestartAfterRecordTaskFailedAsync(restartRecordingReason));
        }
    }

    ///
    private void StartDamakuConnection(bool delay = true) =>
        _ = Task.Run(async () =>
        {
            if (disposedValue)
                return;
            try
            {
                if (delay)
                {
                    try
                    {
                        await Task.Delay((int)RoomConfig.TimingDanmakuRetry, ct).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // 房间已被删除
                        return;
                    }
                }

                // 至少要等到获取到一次房间信息后才能连接弹幕服务器。
                // 理论上不专门写这么个逻辑也应该没有问题的，但为了保证连接时肯定有主播 uid 信息还是加上吧。
                // 这里同步堵塞等待了，因为 ManualResetEventSlim 没有 Async Wait
                // 并且这里运行在一个新的 Task 里，同步等待的影响也不是很大。
                if (!danmakuConnectHoldOff.Wait(TimeSpan.FromSeconds(2)))
                {
                    // 如果 2 秒后还没有获取到房间信息直接返回
                    // 房间信息获取成功后会自动再次尝试连接弹幕服务器所以不用担心会导致不连接弹幕服务器
                    logger.Debug("暂无房间信息，不连接弹幕服务器");
                    return;
                }

                await danmakuClient
                    .ConnectAsync(RoomConfig.RoomId, RoomConfig.DanmakuTransport, ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Write(ex is ExecutionRejectedException ? LogEventLevel.Verbose : LogEventLevel.Warning,
                    ex, "连接弹幕服务器时出错");

                if (!ct.IsCancellationRequested)
                    StartDamakuConnection(delay: true);
            }
        });

    #endregion

    #region Event Handlers

    ///
    private void RecordTask_IOStats(object? sender, IOStatsEventArgs e)
    {
        logger.Verbose("IO stats: {@stats}", e);

        dispatchProvider.DispatchToUiThread(() =>
        {
            Stats.StreamHost = e.StreamHost;
            Stats.StartTime = e.StartTime;
            Stats.EndTime = e.EndTime;
            Stats.Duration = e.Duration;
            Stats.NetworkBytesDownloaded = e.NetworkBytesDownloaded;
            Stats.NetworkMbps = e.NetworkMbps;
            Stats.DiskWriteDuration = e.DiskWriteDuration;
            Stats.DiskBytesWritten = e.DiskBytesWritten;
            Stats.DiskMBps = e.DiskMBps;
        });

        IOStats?.Invoke(this, e);
    }

    ///
    private void RecordTask_RecordingStats(object? sender, RecordingStatsEventArgs e)
    {
        logger.Verbose("Recording stats: {@stats}", e);

        Stats.SessionDuration = TimeSpan.FromMilliseconds(e.SessionDuration);
        Stats.TotalInputBytes = e.TotalInputBytes;
        Stats.TotalOutputBytes = e.TotalOutputBytes;
        Stats.CurrentFileSize = e.CurrentFileSize;
        Stats.SessionMaxTimestamp = TimeSpan.FromMilliseconds(e.SessionMaxTimestamp);
        Stats.FileMaxTimestamp = TimeSpan.FromMilliseconds(e.FileMaxTimestamp);
        Stats.AddedDuration = e.AddedDuration;
        Stats.PassedTime = e.PassedTime;
        Stats.DurationRatio = e.DurationRatio;

        Stats.InputVideoBytes = e.InputVideoBytes;
        Stats.InputAudioBytes = e.InputAudioBytes;

        Stats.OutputVideoFrames = e.OutputVideoFrames;
        Stats.OutputAudioFrames = e.OutputAudioFrames;
        Stats.OutputVideoBytes = e.OutputVideoBytes;
        Stats.OutputAudioBytes = e.OutputAudioBytes;

        Stats.TotalInputVideoBytes = e.TotalInputVideoBytes;
        Stats.TotalInputAudioBytes = e.TotalInputAudioBytes;

        Stats.TotalOutputVideoFrames = e.TotalOutputVideoFrames;
        Stats.TotalOutputAudioFrames = e.TotalOutputAudioFrames;
        Stats.TotalOutputVideoBytes = e.TotalOutputVideoBytes;
        Stats.TotalOutputAudioBytes = e.TotalOutputAudioBytes;

        RecordingStats?.Invoke(this, e);
    }

    ///
    private void RecordTask_RecordFileClosed(object? sender, RecordFileClosedEventArgs e)
    {
        basicDanmakuWriter.Disable();

        RecordFileClosed?.Invoke(this, e);
    }

    ///
    private void RecordTask_RecordFileOpening(object? sender, RecordFileOpeningEventArgs e)
    {
        if (RoomConfig.RecordDanmaku)
            basicDanmakuWriter.EnableWithPath(Path.ChangeExtension(e.FullPath, "xml"), this);
        else
            basicDanmakuWriter.Disable();

        if (RoomConfig.SaveStreamCover)
        {
            _ = Task.Run(() => SaveStreamCoverAsync(e.FullPath));
        }

        RecordFileOpening?.Invoke(this, e);
    }

    private async Task SaveStreamCoverAsync(string flvFullPath)
    {
        const int MAX_ATTEMPT = 3;
        var attempt = 0;
        retry:
        try
        {
            var coverUrl = RawApiJsonData?["room_info"]?["cover"]?.ToObject<string>();

            if (string.IsNullOrWhiteSpace(coverUrl))
            {
                logger.Information("没有直播间封面信息");
                return;
            }

            var targetPath = Path.ChangeExtension(flvFullPath, "cover" + Path.GetExtension(coverUrl));

            var stream = await coverDownloadHttpClient.GetStreamAsync(coverUrl).ConfigureAwait(false);
            using var file = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(file).ConfigureAwait(false);

            logger.Debug("直播间封面已成功从 {CoverUrl} 保存到 {FilePath}", coverUrl, targetPath);
        }
        catch (Exception ex)
        {
            if (attempt++ < MAX_ATTEMPT)
            {
                logger.Debug(ex, "保存直播间封面时出错, 次数 {Attempt}", attempt);
                goto retry;
            }

            logger.Warning(ex, "保存直播间封面时出错");
        }
    }

    ///
    private void RecordTask_RecordSessionEnded(object? sender, EventArgs e)
    {
        Guid id;
        lock (recordStartLock)
        {
            id = recordTask?.SessionId ?? Guid.Empty;
            recordTask = null;
            _ = Task.Run(async () =>
            {
                await Task.Yield();

                // 录制结束退出后的重试逻辑
                // 比 RestartAfterRecordTaskFailedAsync 少了等待时间

                // 如果状态是非直播中，跳过重试尝试。当状态切换到直播中时会开始新的录制任务。
                if (!Streaming || !AutoRecordForThisSession)
                    return;

                try
                {
                    // 开始录制前刷新房间信息
                    if (Streaming && AutoRecordForThisSession)
                        CreateAndStartNewRecordTask(skipFetchRoomInfo: false);
                }
                catch (Exception ex)
                {
                    logger.Write(LogEventLevel.Warning, ex, "重试开始录制时出错");
                    _ = Task.Run(() => RestartAfterRecordTaskFailedAsync(RestartRecordingReason.GenericRetry));
                }
            });
        }

        basicDanmakuWriter.Disable();

        OnPropertyChanged(nameof(Recording));
        Stats.Reset();

        RecordSessionEnded?.Invoke(this, new RecordSessionEndedEventArgs(this)
        {
            SessionId = id
        });
    }

    private string? DanmakuClient_BeforeHandshake(string json)
    {
        var danmakuAuthenticateWithStreamerUid = RoomConfig.DanmakuAuthenticateWithStreamerUid;
        if (danmakuAuthenticateWithStreamerUid)
        {
            var obj = JObject.Parse(json);
            obj["uid"] = Uid;
            obj.Remove("key");
            obj.Remove("buvid");
            json = obj.ToString(Formatting.None);
        }

        var scriptUpdatedJson = userScriptRunner.CallOnDanmakuHandshake(logger, this, json);

        if (scriptUpdatedJson is not null)
            return scriptUpdatedJson;
        else if (danmakuAuthenticateWithStreamerUid)
            return json;
        else
            return null;
    }

    private void DanmakuClient_DanmakuReceived(object? sender, DanmakuReceivedEventArgs e)
    {
        var d = e.Danmaku;

        switch (d.MsgType)
        {
            case DanmakuMsgType.LiveStart:
                logger.Debug("推送直播开始");
                Streaming = true;
                break;
            case DanmakuMsgType.LiveEnd:
                logger.Debug("推送直播结束");
                Streaming = false;
                break;
            //     case DanmakuMsgType.RoomChange:
            //         logger.Debug("推送房间信息变更, {Title}, {AreaNameParent}, {AreaNameChild}", d.Title, d.ParentAreaName,
            //             d.AreaName);
            //         Title = d.Title ?? Title;
            //         AreaNameParent = d.ParentAreaName ?? AreaNameParent;
            //         AreaNameChild = d.AreaName ?? AreaNameChild;
            //         break;
            case DanmakuMsgType.RoomLock:
                logger.Information("直播间被封禁");
                break;
            case DanmakuMsgType.CutOff:
                logger.Information("直播被管理员切断");
                break;
        }

        _ = Task.Run(async () => await basicDanmakuWriter.WriteAsync(d), ct);
    }

    private void DanmakuClient_StatusChanged(object? sender, StatusChangedEventArgs e)
    {
        DanmakuConnected = e.Connected;
        if (e.Connected)
        {
            danmakuClientConnectTime = DateTimeOffset.UtcNow;
            logger.Information("弹幕服务器已连接");
        }
        else
        {
            logger.Information("与弹幕服务器的连接被断开");

            // 如果连接弹幕服务器的时间在至少 1 分钟之前，重连时不需要等待
            // 针对偶尔的网络波动的优化，如果偶尔断开了尽快重连，减少漏录的弹幕量
            StartDamakuConnection(delay: !((DateTimeOffset.UtcNow - danmakuClientConnectTime) >
                                           danmakuClientReconnectNoDelay));
            danmakuClientConnectTime = DateTimeOffset.MaxValue;
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        StartDamakuConnection(delay: false);

        // 如果开启了自动录制 或者 还没有获取过第一次房间信息
        if (RoomConfig.AutoRecord || !danmakuConnectHoldOff.IsSet)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // 定时主动检查不需要错误重试
                    await FetchRoomInfoAsync().ConfigureAwait(false);

                    // 刚更新了房间信息不需要再获取一次
                    if (Streaming && AutoRecordForThisSession && RoomConfig.AutoRecord)
                        CreateAndStartNewRecordTask(skipFetchRoomInfo: true);
                }
                catch (Exception)
                {
                }
            }, ct);
        }
    }


    private void Room_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Streaming):
                if (Streaming)
                {
                    // 如果开播状态是通过广播消息获取的，本地的直播间信息就不是最新的，需要重新获取。
                    if (AutoRecordForThisSession && RoomConfig.AutoRecord)
                        CreateAndStartNewRecordTask(skipFetchRoomInfo: false);
                }
                else
                {
                    AutoRecordForThisSession = true;
                }

                break;
            case nameof(Title):
                if (RoomConfig.CuttingByTitle)
                {
                    SplitOutput();
                }

                break;
            default:
                break;
        }
    }

    private void RoomConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(RoomConfig.RoomId):
                logger = loggerWithoutContext.ForContext(LoggingContext.RoomId, RoomConfig.RoomId);
                break;
            case nameof(RoomConfig.TimingCheckInterval):
                timer.Interval = RoomConfig.TimingCheckInterval * 1000d;
                break;
            case nameof(RoomConfig.AutoRecord):
                if (RoomConfig.AutoRecord)
                {
                    AutoRecordForThisSession = true;

                    // 启动录制时更新一次房间信息
                    if (Streaming && AutoRecordForThisSession)
                        CreateAndStartNewRecordTask(skipFetchRoomInfo: false);
                }

                break;
            default:
                break;
        }
    }

    #endregion

    #region PropertyChanged

    protected void SetField<T>(ref T location, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(location, value))
            return;

        location = value;

        if (propertyName != null)
            OnPropertyChanged(propertyName);
    }

    protected void OnPropertyChanged(string propertyName) =>
        dispatchProvider.DispatchToUiThread(() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName)));

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
            if (disposing)
            {
                // dispose managed state (managed objects)
                cts.Cancel();
                cts.Dispose();
                recordTask?.RequestStop();
                basicDanmakuWriter.Disable();
                scope.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
        }
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Room()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
