using System.IO.Pipelines;

namespace BililiveRecorder.Common.Api.Danmaku;

public interface IDanmakuTransport : IDisposable
{
    Task<PipeReader> ConnectAsync(Uri uri, CancellationToken cancellationToken);
    Task SendAsync(byte[] buffer, int offset, int count);
}
