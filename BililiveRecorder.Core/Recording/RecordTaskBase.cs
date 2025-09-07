using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Event;
using BililiveRecorder.Common.Scripting;
using BililiveRecorder.Core.Templating;
using Serilog;
using IApiClient = BililiveRecorder.Common.Api.IApiClient;
using Timer = System.Timers.Timer;

namespace BililiveRecorder.Core.Recording;

internal abstract class RecordTaskBase : IRecordTask
{
    private const int timer_inverval = 2;
    protected readonly Timer timer = new Timer(1000 * timer_inverval);
    protected readonly Random random = new Random();
    protected readonly CancellationTokenSource cts = new();
    protected readonly CancellationToken ct;

    protected readonly IRoom room;
    protected readonly ILogger logger;
    protected readonly IApiClient apiClient;
    private readonly FileNameGenerator fileNameGenerator;
    private readonly UserScriptRunner userScriptRunner;
    protected readonly IDispatchProvider _dispatchProvider;

    private int partIndex = 0;

    protected string? streamHost;
    protected string? streamHostFull;
    protected bool started = false;
    protected bool timeoutTriggered = false;
    protected int qn;

    private readonly object ioStatsLock = new();
    protected int ioNetworkDownloadedBytes;

    protected Stopwatch ioDiskStopwatch = new();
    protected object ioDiskStatsLock = new();
    protected TimeSpan ioDiskWriteDuration;
    protected int ioDiskWrittenBytes;

    private DateTimeOffset ioStatsLastTrigger;
    private TimeSpan durationSinceNoDataReceived;

    protected RecordTaskBase(IRoom room, ILogger logger, IApiClient apiClient, UserScriptRunner userScriptRunner, IDispatchProvider dispatchProvider)
    {
        this.room = room ?? throw new ArgumentNullException(nameof(room));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.userScriptRunner = userScriptRunner ?? throw new ArgumentNullException(nameof(userScriptRunner));
        _dispatchProvider = dispatchProvider ?? throw new ArgumentNullException(nameof(dispatchProvider));

        fileNameGenerator = new FileNameGenerator(room.RoomConfig, logger);
        ct = cts.Token;

        timer.Elapsed += Timer_Elapsed_TriggerIOStats;
    }

    public Guid SessionId { get; } = Guid.NewGuid();

    #region Events

    public event EventHandler<IOStatsEventArgs>? IOStats;
    public event EventHandler<RecordingStatsEventArgs>? RecordingStats;
    public event EventHandler<RecordFileOpeningEventArgs>? RecordFileOpening;
    public event EventHandler<RecordFileClosedEventArgs>? RecordFileClosed;
    public event EventHandler? RecordSessionEnded;

    protected void OnIOStats(IOStatsEventArgs e) => _dispatchProvider.DispatchToUiThread(() => IOStats?.Invoke(this, e));
    protected void OnRecordingStats(RecordingStatsEventArgs e) => _dispatchProvider.DispatchToUiThread(() => RecordingStats?.Invoke(this, e));
    protected void OnRecordFileOpening(RecordFileOpeningEventArgs e) => RecordFileOpening?.Invoke(this, e);
    protected void OnRecordFileClosed(RecordFileClosedEventArgs e) => RecordFileClosed?.Invoke(this, e);
    protected void OnRecordSessionEnded(EventArgs e) => _dispatchProvider.DispatchToUiThread(() => RecordSessionEnded?.Invoke(this, e));

    #endregion

    public virtual void RequestStop() => cts.Cancel();

    public virtual void SplitOutput()
    {
    }

    public virtual async Task StartAsync()
    {
        if (started)
            throw new InvalidOperationException("Only one StartAsync call allowed per instance.");
        started = true;

        var streamInfo = await apiClient.GetStreamUrlAsync(room.RoomConfig.RoomId, room.RoomConfig.RecordingQuality).ConfigureAwait(false);

        qn = streamInfo.Qn;
        streamHost = new Uri(streamInfo.Url).Host;

        logger.Information("连接直播服务器 {Host} 录制画质 {Qn} ({QnDescription})", streamHost, qn, streamInfo.QnName);
        logger.Debug("直播流地址 {Url}", streamInfo.Url);

        var stream = await GetStreamAsync(fullUrl: streamInfo.Url, timeout: (int)room.RoomConfig.TimingStreamConnect).ConfigureAwait(false);

        ioStatsLastTrigger = DateTimeOffset.UtcNow;
        durationSinceNoDataReceived = TimeSpan.Zero;

        ct.Register(state => _ = Task.Run(async () =>
        {
            try
            {
                if (state is not WeakReference<Stream> weakRef)
                    return;

                await Task.Delay(1000);

                if (weakRef.TryGetTarget(out var weakStream))
                {
#if NET6_0_OR_GREATER
                    await weakStream.DisposeAsync();
#else
                        weakStream.Dispose();
#endif
                }
            }
            catch (Exception)
            {
            }
        }), state: new WeakReference<Stream>(stream), useSynchronizationContext: false);

        StartRecordingLoop(stream);
    }

    protected abstract void StartRecordingLoop(Stream stream);

    private void Timer_Elapsed_TriggerIOStats(object? sender, ElapsedEventArgs e)
    {
        int networkDownloadBytes, diskWriteBytes;
        TimeSpan durationDiff, diskWriteDuration;
        DateTimeOffset startTime, endTime;


        lock (ioStatsLock) // 锁 timer elapsed 事件本身防止并行运行
        {
            // networks
            networkDownloadBytes = Interlocked.Exchange(ref ioNetworkDownloadedBytes, 0); // 锁网络统计
            endTime = DateTimeOffset.UtcNow;
            startTime = ioStatsLastTrigger;
            ioStatsLastTrigger = endTime;
            durationDiff = endTime - startTime;

            durationSinceNoDataReceived = networkDownloadBytes > 0 ? TimeSpan.Zero : durationSinceNoDataReceived + durationDiff;

            // disks
            lock (ioDiskStatsLock) // 锁硬盘统计
            {
                diskWriteDuration = ioDiskWriteDuration;
                diskWriteBytes = ioDiskWrittenBytes;
                ioDiskWriteDuration = TimeSpan.Zero;
                ioDiskWrittenBytes = 0;
            }
        }

        var netMbps = networkDownloadBytes * (8d / 1024d / 1024d) / durationDiff.TotalSeconds;
        var diskMBps = diskWriteBytes / (1024d * 1024d) / diskWriteDuration.TotalSeconds;

        OnIOStats(new IOStatsEventArgs
        {
            StreamHost = streamHost,
            NetworkBytesDownloaded = networkDownloadBytes,
            Duration = durationDiff,
            StartTime = startTime,
            EndTime = endTime,
            NetworkMbps = netMbps,
            DiskBytesWritten = diskWriteBytes,
            DiskWriteDuration = diskWriteDuration,
            DiskMBps = diskMBps,
        });

        if ((!timeoutTriggered) && (durationSinceNoDataReceived.TotalMilliseconds > room.RoomConfig.TimingWatchdogTimeout))
        {
            timeoutTriggered = true;
            logger.Warning("检测到录制卡住，可能是网络或硬盘原因，将会主动断开连接");
            RequestStop();
        }
    }

    protected (string fullPath, string relativePath) CreateFileName()
    {
        partIndex++;

        var output = fileNameGenerator.CreateFilePath(new FileNameTemplateContext
        {
            Name = FileNameGenerator.RemoveInvalidFileName(room.Name, ignore_slash: false),
            Title = FileNameGenerator.RemoveInvalidFileName(room.Title, ignore_slash: false),
            RoomId = room.RoomConfig.RoomId,
            ShortId = room.ShortId,
            Uid = room.Uid,
            AreaParent = FileNameGenerator.RemoveInvalidFileName(room.AreaNameParent, ignore_slash: false),
            AreaChild = FileNameGenerator.RemoveInvalidFileName(room.AreaNameChild, ignore_slash: false),
            PartIndex = partIndex,
            Qn = qn,
            Json = room.RawApiJsonData,
        });

        return (output.FullPath!, output.RelativePath);
    }

    #region Api Requests

    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseProxy = room.RoomConfig.NetworkTransportUseSystemProxy,
        });
        var headers = httpClient.DefaultRequestHeaders;
        foreach (var header in apiClient.Headers)
        {
            headers.Add(header.Key, header.Value);
        }

        return httpClient;
    }

    protected async Task<Stream> GetStreamAsync(string fullUrl, int timeout)
    {
        var client = CreateHttpClient();

        var streamHostInfoBuilder = new StringBuilder();

        while (true)
        {
            var allowedAddressFamily = room.RoomConfig.NetworkTransportAllowedAddressFamily;
            HttpRequestMessage request;
            Uri originalUri;

            if (userScriptRunner.CallOnTransformStreamUrl(logger, fullUrl) is { } scriptResult)
            {
                var (scriptUrl, scriptIp) = scriptResult;

                logger.Debug("用户脚本重定向了直播流地址 {NewUrl}, 旧地址 {OldUrl}", scriptUrl, fullUrl);

                fullUrl = scriptUrl;
                originalUri = new Uri(fullUrl);


                if (scriptIp is not null)
                {
                    logger.Debug("用户脚本指定了服务器 IP {IP}", scriptIp);

                    var uri = new Uri(fullUrl);
                    var builder = new UriBuilder(uri)
                    {
                        Host = scriptIp
                    };

                    request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
                    request.Headers.Host = uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port;

                    streamHostInfoBuilder.Append(originalUri.Host);
                    streamHostInfoBuilder.Append(" [");
                    streamHostInfoBuilder.Append(scriptIp);
                    streamHostInfoBuilder.Append(']');

                    goto sendRequest;
                }
            }
            else
            {
                originalUri = new Uri(fullUrl);
            }

            if (allowedAddressFamily == AllowedAddressFamily.System)
            {
                logger.Debug("NetworkTransportAllowedAddressFamily is System");
                request = new HttpRequestMessage(HttpMethod.Get, originalUri);

                streamHostInfoBuilder.Append(originalUri.Host);
            }
            else
            {
                var ips = await Dns.GetHostAddressesAsync(originalUri.DnsSafeHost, ct);

                var filtered = ips.Where(x => allowedAddressFamily switch
                {
                    AllowedAddressFamily.Ipv4 => x.AddressFamily == AddressFamily.InterNetwork,
                    AllowedAddressFamily.Ipv6 => x.AddressFamily == AddressFamily.InterNetworkV6,
                    AllowedAddressFamily.Any => true,
                    _ => false
                }).ToArray();

                var selected = filtered[random.Next(filtered.Length)];

                logger.Debug("指定直播服务器地址 {DnsHost}: {SelectedIp}, Allowed: {AllowedAddressFamily}, {IPAddresses}", originalUri.DnsSafeHost, selected, allowedAddressFamily,
                    ips);

                streamHostInfoBuilder.Append(originalUri.Host);
                streamHostInfoBuilder.Append(" [");
                streamHostInfoBuilder.Append(selected);
                streamHostInfoBuilder.Append(']');

                if (selected is null)
                {
                    throw new Exception("DNS 没有返回符合要求的 IP 地址");
                }

                var builder = new UriBuilder(originalUri)
                {
                    Host = selected.ToString()
                };

                request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
                request.Headers.Host = originalUri.IsDefaultPort ? originalUri.Host : originalUri.Host + ":" + originalUri.Port;
            }

            sendRequest:

            var resp = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    new CancellationTokenSource(timeout).Token)
                .ConfigureAwait(false);

            switch (resp.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    logger.Information("开始接收直播流");
                    streamHostFull = streamHostInfoBuilder.ToString();
                    var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return stream;
                }
                case HttpStatusCode.Moved:
                case HttpStatusCode.Redirect:
                {
                    fullUrl = new Uri(originalUri, resp.Headers.Location!).ToString();
                    logger.Debug("跳转到 {Url}, 原文本 {Location}", fullUrl, resp.Headers.Location!.OriginalString);
                    resp.Dispose();
                    streamHostInfoBuilder.Append('\n');
                    break;
                }
                default:
                    throw new Exception($"尝试下载直播流时服务器返回了 ({resp.StatusCode}){resp.ReasonPhrase}");
            }
        }
    }

    #endregion
}
