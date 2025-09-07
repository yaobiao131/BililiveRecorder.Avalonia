using System.Buffers;
using System.IO.Pipelines;
using System.Timers;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Common.Config;
using Serilog;

namespace BililiveRecorder.Common;

public abstract class BaseDanmakuClient : IDanmakuClient
{
    private readonly ILogger logger;
    private readonly System.Timers.Timer timer;
    private IDanmakuServerApiClient apiClient;
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    protected IDanmakuTransport? danmakuTransport;
    private bool disposedValue;

    protected virtual int HeartBeatInterval => 30;

    protected abstract byte[] PingMessage { get; }

    protected BaseDanmakuClient(IDanmakuServerApiClient apiClient, ILogger logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        timer = new System.Timers.Timer(interval: 1000 * HeartBeatInterval)
        {
            AutoReset = true,
            Enabled = false
        };
        timer.Elapsed += SendPingMessageTimerCallback;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                timer.Dispose();
                danmakuTransport?.Dispose();
                semaphoreSlim.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool Connected => danmakuTransport != null;
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DanmakuReceivedEventArgs>? DanmakuReceived;
    public Func<string, string?>? BeforeHandshake { get; set; }

    public async Task ConnectAsync(long roomId, DanmakuTransportMode transportMode, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(disposedValue, nameof(BaseDanmakuClient));
        if (!Enum.IsDefined(typeof(DanmakuTransportMode), transportMode))
            throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, "Invalid danmaku transport mode.");
        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (danmakuTransport != null)
                return;
            var serverInfo = await apiClient.GetDanmakuServerAsync(roomId).ConfigureAwait(false);

            logger.Debug("连接弹幕服务器 {Mode} {Host}:{Port} 房间: {RoomId}", serverInfo.Transport, serverInfo.Url.Host, serverInfo.Url.Port, roomId);

            IDanmakuTransport transport = serverInfo.Transport switch
            {
                DanmakuTransportMode.Tcp => new DanmakuTransportTcp(),
                DanmakuTransportMode.Ws => new DanmakuTransportWebSocket(apiClient.DanmakuHeaders),
                DanmakuTransportMode.Wss => new DanmakuTransportSecureWebSocket(apiClient.DanmakuHeaders),
                _ => throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, "Invalid danmaku transport mode."),
            };

            var reader = await transport.ConnectAsync(serverInfo.Url, cancellationToken).ConfigureAwait(false);

            danmakuTransport = transport;

            foreach (var message in serverInfo.RegMessage)
            {
                await danmakuTransport.SendAsync(message, 0, message.Length).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                danmakuTransport?.Dispose();
                danmakuTransport = null;
                return;
            }

            timer.Start();

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessDataAsync(reader).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "Error running ProcessDataAsync");
                }

                try
                {
                    await DisconnectAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                }
            }, CancellationToken.None);
        }
        finally
        {
            semaphoreSlim.Release();
        }

        StatusChanged?.Invoke(this, StatusChangedEventArgs.True);
    }

    public async Task DisconnectAsync()
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            danmakuTransport?.Dispose();
            danmakuTransport = null;

            timer.Stop();
        }
        finally
        {
            semaphoreSlim.Release();
        }

        StatusChanged?.Invoke(this, StatusChangedEventArgs.False);
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void SendPingMessageTimerCallback(object? sender, ElapsedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                if (danmakuTransport is null)
                    return;

                await danmakuTransport.SendAsync(PingMessage, 0, PingMessage.Length).ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error running SendPingMessageTimerCallback");
        }
    }

    #region Receive

    private async Task ProcessDataAsync(PipeReader reader)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;
            var (danmakuList, length) = await DecodeMessageAsync(buffer).ConfigureAwait(false);
            foreach (var danmaku in danmakuList)
            {
                DanmakuReceived?.Invoke(this, new DanmakuReceivedEventArgs(danmaku));
            }

            buffer = buffer.Slice(length);

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    #endregion

    protected abstract Task<Tuple<List<BaseDanmakeModel>, SequencePosition>> DecodeMessageAsync(ReadOnlySequence<byte> bytes);
}
