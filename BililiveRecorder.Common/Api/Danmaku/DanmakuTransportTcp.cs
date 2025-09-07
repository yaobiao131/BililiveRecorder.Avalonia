using System.IO.Pipelines;
using System.Net.Sockets;
using Nerdbank.Streams;

namespace BililiveRecorder.Common.Api.Danmaku
{
    public class DanmakuTransportTcp : IDanmakuTransport
    {
        private Stream? stream;

        public async Task<PipeReader> ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (stream is not null)
                throw new InvalidOperationException("Tcp socket is connected.");

            var tcp = new TcpClient();
            await tcp.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);

            var networkStream = tcp.GetStream();
            stream = networkStream;
            return networkStream.UsePipeReader();
        }

        public void Dispose() => stream?.Dispose();

        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            if (stream is not { } s)
                return;

            await s.WriteAsync(buffer, offset, count).ConfigureAwait(false);
            await s.FlushAsync().ConfigureAwait(false);
        }
    }
}
