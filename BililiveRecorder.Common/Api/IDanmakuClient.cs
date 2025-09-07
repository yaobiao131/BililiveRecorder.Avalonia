using BililiveRecorder.Common.Api.Danmaku;
using BililiveRecorder.Common.Config;

namespace BililiveRecorder.Common.Api
{
    public interface IDanmakuClient : IDisposable
    {
        bool Connected { get; }

        event EventHandler<StatusChangedEventArgs>? StatusChanged;
        event EventHandler<DanmakuReceivedEventArgs>? DanmakuReceived;

        Func<string, string?>? BeforeHandshake { get; set; }

        Task ConnectAsync(long roomid, DanmakuTransportMode transportMode, CancellationToken cancellationToken);
        Task DisconnectAsync();
    }
}
