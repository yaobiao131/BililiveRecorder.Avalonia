using System.Buffers;
using System.Buffers.Binary;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Timers;
using BililiveRecorder.BiliBili.Model;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Common.Config;
using Microsoft.Extensions.DependencyInjection;
using Nerdbank.Streams;
using Newtonsoft.Json;
using Serilog;
using Timer = System.Timers.Timer;

namespace BililiveRecorder.BiliBili;

internal class BiliBiliDanmakuClient : IDanmakuClient, IDisposable
{
    private readonly ILogger logger;
    private readonly IBiliBiliDanmakuServerApiClient apiClient;
    private readonly Timer timer;
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    private IDanmakuTransport? danmakuTransport;
    private bool disposedValue;

    public bool Connected => danmakuTransport != null;

    public event EventHandler<StatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DanmakuReceivedEventArgs>? DanmakuReceived;

    public Func<string, string?>? BeforeHandshake { get; set; } = null;

    private static readonly JsonSerializerSettings jsonSerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore };

    public BiliBiliDanmakuClient([FromKeyedServices(Platform.BiliBili)] IDanmakuServerApiClient apiClient, ILogger logger)
    {
        this.apiClient = (IBiliBiliDanmakuServerApiClient)apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger.ForContext<BiliBiliDanmakuClient>() ?? throw new ArgumentNullException(nameof(logger));

        timer = new Timer(interval: 1000 * 30)
        {
            AutoReset = true,
            Enabled = false
        };
        timer.Elapsed += SendPingMessageTimerCallback;
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

    public async Task ConnectAsync(long roomId, DanmakuTransportMode transportMode, CancellationToken cancellationToken)
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(BiliBiliDanmakuClient));

        if (!Enum.IsDefined(typeof(DanmakuTransportMode), transportMode))
            throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, "Invalid danmaku transport mode.");

        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (danmakuTransport != null)
                return;

            var serverInfo = (DanmuInfo)await apiClient.GetDanmakuServerAsync(roomId).ConfigureAwait(false);
            if (serverInfo is null)
                return;

            var danmakuServerInfo = (DanmuInfo)serverInfo.SelectDanmakuServer(transportMode);

            logger.Debug("连接弹幕服务器 {Mode} {Host}:{Port} 房间: {RoomId} TokenLength: {TokenLength}", danmakuServerInfo.Transport, danmakuServerInfo.Url.Host,
                danmakuServerInfo.Url.Port, roomId, danmakuServerInfo.Token?.Length);

            IDanmakuTransport transport = danmakuServerInfo.Transport switch
            {
                DanmakuTransportMode.Tcp => new DanmakuTransportTcp(),
                DanmakuTransportMode.Ws => new DanmakuTransportWebSocket(apiClient.DanmakuHeaders),
                DanmakuTransportMode.Wss => new DanmakuTransportSecureWebSocket(apiClient.DanmakuHeaders),
                _ => throw new ArgumentOutOfRangeException(nameof(transportMode), transportMode, "Invalid danmaku transport mode."),
            };

            var reader = await transport.ConnectAsync(danmakuServerInfo.Url, cancellationToken).ConfigureAwait(false);

            danmakuTransport = transport;

            await SendHelloAsync(roomId, apiClient.GetUid(), apiClient.GetBuvid3(), danmakuServerInfo.Token ?? string.Empty).ConfigureAwait(false);
            await SendPingAsync().ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                danmakuTransport.Dispose();
                danmakuTransport = null;
                return;
            }

            timer.Start();

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessDataAsync(reader, ProcessCommand).ConfigureAwait(false);
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

    private void ProcessCommand(string json)
    {
        try
        {
            var d = new BiliBiliDanmakuModel(json);
            DanmakuReceived?.Invoke(this, new DanmakuReceivedEventArgs(d));
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Error running ProcessCommand");
        }
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

                await SendPingAsync().ConfigureAwait(false);
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

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                timer.Dispose();
                danmakuTransport?.Dispose();
                semaphoreSlim.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DanmakuClient()
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

    #region Send

    private Task SendHelloAsync(long roomid, long uid, string? buvid, string token)
    {
        var body = JsonConvert.SerializeObject(new
        {
            uid,
            roomid,
            protover = 3,
            buvid,
            platform = "web",
            type = 2,
            key = token,
        }, Formatting.None, jsonSerializerSettings);

        if (BeforeHandshake is { } func)
        {
            var newBody = func(body);
            if (newBody is not null)
            {
                logger.Debug("Danmaku BeforeHandshake: {OldBody} => {NewBody}", body, newBody);
                body = newBody;
            }
        }

        return SendMessageAsync(7, body);
    }

    private Task SendPingAsync() => SendMessageAsync(2);

    private async Task SendMessageAsync(int action, string body = "")
    {
        if (danmakuTransport is not { } transport)
            return;

        var payload = Encoding.UTF8.GetBytes(body);
        const int headerLength = 16;
        var totalLength = payload.Length + headerLength;

        var buffer = ArrayPool<byte>.Shared.Rent(totalLength);
        try
        {
            BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(buffer, 0, 4), (uint)totalLength);
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(buffer, 4, 2), headerLength);
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(buffer, 6, 2), 1);
            BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(buffer, 8, 4), (uint)action);
            BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(buffer, 12, 4), 1);

            if (payload.Length > 0)
                Array.Copy(payload, 0, buffer, headerLength, payload.Length);

            await transport.SendAsync(buffer, 0, totalLength).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #endregion

    #region Receive

    private static async Task ProcessDataAsync(PipeReader reader, Action<string> callback)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            while (TryParseCommand(ref buffer, callback))
            {
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    private static bool TryParseCommand(ref ReadOnlySequence<byte> buffer, Action<string> callback)
    {
        if (buffer.Length < 4)
            return false;

        int length;
        {
            var lengthSlice = buffer.Slice(buffer.Start, 4);
            if (lengthSlice.IsSingleSegment)
            {
                length = BinaryPrimitives.ReadInt32BigEndian(lengthSlice.First.Span);
            }
            else
            {
                Span<byte> stackBuffer = stackalloc byte[4];
                lengthSlice.CopyTo(stackBuffer);
                length = BinaryPrimitives.ReadInt32BigEndian(stackBuffer);
            }
        }

        if (buffer.Length < length)
            return false;

        var headerSlice = buffer.Slice(buffer.Start, 16);
        buffer = buffer.Slice(headerSlice.End);
        var bodySlice = buffer.Slice(buffer.Start, length - 16);
        buffer = buffer.Slice(bodySlice.End);

        DanmakuProtocol header;
        if (headerSlice.IsSingleSegment)
        {
            Parse2Protocol(headerSlice.First.Span, out header);
        }
        else
        {
            Span<byte> stackBuffer = stackalloc byte[16];
            headerSlice.CopyTo(stackBuffer);
            Parse2Protocol(stackBuffer, out header);
        }

        if (header.Version == 2 && header.Action == 5)
        {
            using var deflate = new DeflateStream(bodySlice.Slice(2, bodySlice.End).AsStream(), CompressionMode.Decompress, leaveOpen: false);
            ParseCommandCompressedBody(deflate, callback);
        }
        else if (header.Version == 3 && header.Action == 5)
        {
#if NET6_0_OR_GREATER
            using var brotli = new BrotliStream(bodySlice.AsStream(), CompressionMode.Decompress, leaveOpen: false);
#else
                using var brotli = new BrotliSharpLib.BrotliStream(bodySlice.AsStream(), CompressionMode.Decompress, leaveOpen: false);
#endif
            ParseCommandCompressedBody(brotli, callback);
        }
        else
            ParseCommandNormalBody(ref bodySlice, header.Action, callback);

        return true;
    }

    private static void ParseCommandCompressedBody(Stream decompressed, Action<string> callback)
    {
        var reader = PipeReader.Create(decompressed);
        while (true)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            // 全内存内运行同步返回，所以不会有问题
            var result = reader.ReadAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            var inner_buffer = result.Buffer;

            while (TryParseCommand(ref inner_buffer, callback))
            {
            }

            reader.AdvanceTo(inner_buffer.Start, inner_buffer.End);

            if (result.IsCompleted)
                break;
        }

        reader.Complete();
    }

    private static void ParseCommandNormalBody(ref ReadOnlySequence<byte> buffer, int action, Action<string> callback)
    {
        switch (action)
        {
            case 5:
            {
                if (buffer.Length > int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "ParseCommandNormalBody buffer length larger than int.MaxValue");

                var b = ArrayPool<byte>.Shared.Rent((int)buffer.Length);
                try
                {
                    buffer.CopyTo(b);
                    var json = Encoding.UTF8.GetString(b, 0, (int)buffer.Length);
                    callback(json);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(b);
                }
            }
                break;
            case 3:

                break;
            default:
                break;
        }
    }

    private static unsafe void Parse2Protocol(ReadOnlySpan<byte> buffer, out DanmakuProtocol protocol)
    {
        fixed (byte* ptr = buffer)
        {
            protocol = *(DanmakuProtocol*)ptr;
        }

        protocol.ChangeEndian();
    }

    private struct DanmakuProtocol
    {
        /// <summary>
        /// 消息总长度 (协议头 + 数据长度)
        /// </summary>
        public int PacketLength;

        /// <summary>
        /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
        /// </summary>
        public short HeaderLength;

        /// <summary>
        /// 消息版本号
        /// </summary>
        public short Version;

        /// <summary>
        /// 消息类型
        /// </summary>
        public int Action;

        /// <summary>
        /// 参数, 固定为1
        /// </summary>
        public int Parameter;

        /// <summary>
        /// 转为本机字节序
        /// </summary>
        public void ChangeEndian()
        {
            PacketLength = IPAddress.HostToNetworkOrder(PacketLength);
            HeaderLength = IPAddress.HostToNetworkOrder(HeaderLength);
            Version = IPAddress.HostToNetworkOrder(Version);
            Action = IPAddress.HostToNetworkOrder(Action);
            Parameter = IPAddress.HostToNetworkOrder(Parameter);
        }
    }

    #endregion
}
