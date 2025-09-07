using System.Collections;
using System.IO.Pipelines;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using Nerdbank.Streams;

namespace BililiveRecorder.Common.Api.Danmaku
{
    public class DanmakuTransportWebSocket : IDanmakuTransport
    {
        private readonly ClientWebSocket socket;

        protected virtual string Scheme => "ws";

        static DanmakuTransportWebSocket()
        {
            if (!RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal))
                return;

            var headerInfoTable = typeof(WebHeaderCollection).Assembly.GetType("System.Net.HeaderInfoTable", false);
            if (headerInfoTable is null) return;

            var headerHashTable = headerInfoTable.GetField("HeaderHashTable", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (headerHashTable is null) return;

            if (headerHashTable.GetValue(null) is not Hashtable table) return;

            foreach (var key in new[] { "User-Agent", "Referer", "Accept" })
            {
                var info = table[key];
                if (info is null) continue;

                var isRequestRestrictedProperty =
                    info.GetType().GetField("IsRequestRestricted", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (isRequestRestrictedProperty is null) continue;

                isRequestRestrictedProperty.SetValue(info, false);
            }
        }

        public DanmakuTransportWebSocket(Dictionary<string, string>? headers = null)
        {
            socket = new ClientWebSocket();
            var options = socket.Options;
            options.UseDefaultCredentials = false;
            options.Credentials = null;
            options.Cookies = null;
            foreach (var header in headers ?? new Dictionary<string, string>())
            {
                options.SetRequestHeader(header.Key, header.Value);
            }
        }

        public async Task<PipeReader> ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            // 连接超时 10 秒
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            await socket.ConnectAsync(uri, cts.Token).ConfigureAwait(false);
            return socket.UsePipeReader();
        }

        public async Task SendAsync(byte[] buffer, int offset, int count)
            => await socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, true, default).ConfigureAwait(false);

        public void Dispose() => socket.Dispose();
    }
}
