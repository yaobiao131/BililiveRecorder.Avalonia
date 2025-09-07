namespace BililiveRecorder.Common.Api.Danmaku
{
    public class DanmakuTransportSecureWebSocket(Dictionary<string, string>? headers = null) : DanmakuTransportWebSocket(headers)
    {
        protected override string Scheme => "wss";
    }
}
