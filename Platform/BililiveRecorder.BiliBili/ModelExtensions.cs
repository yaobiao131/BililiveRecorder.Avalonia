using BililiveRecorder.BiliBili.Model;
using BililiveRecorder.Common.Api.Model;
using BililiveRecorder.Common.Config;

namespace BililiveRecorder.BiliBili;

public static class ModelExtensions
{
    private static readonly Random random = new Random();

    private const string DefaultServerHost = "broadcastlv.chat.bilibili.com";

    private static readonly DanmakuServerInfo[] DefaultServers = new[]
    {
        new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Tcp, Host = DefaultServerHost, Port = 2243 },
        new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Ws, Host = DefaultServerHost, Port = 2244 },
        new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Wss, Host = DefaultServerHost, Port = 443 },
    };

    internal static BaseDanmuInfo SelectDanmakuServer(this DanmuInfo danmuInfo, DanmakuTransportMode transportMode)
    {
        var result = DefaultServers[random.Next(DefaultServers.Length)];

        static IEnumerable<DanmakuServerInfo> SelectServerInfo(DanmuInfo.HostListItem x)
        {
            yield return new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Tcp, Host = x.Host, Port = x.Port };
            yield return new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Ws, Host = x.Host, Port = x.WsPort };
            yield return new DanmakuServerInfo { TransportMode = DanmakuTransportMode.Wss, Host = x.Host, Port = x.WssPort };
        }

        var list = danmuInfo.HostList.Where(x => !string.IsNullOrWhiteSpace(x.Host) && x.Host != DefaultServerHost)
            .SelectMany(SelectServerInfo)
            .Where(x => x.Port > 0)
            .Where(x => transportMode == DanmakuTransportMode.Random || transportMode == x.TransportMode)
            .ToArray();
        if (list.Length > 0)
        {
            result = list[random.Next(list.Length)];
            result.Token = danmuInfo.Token;
        }

        var scheme = result.TransportMode switch
        {
            DanmakuTransportMode.Tcp => "tcp",
            DanmakuTransportMode.Ws => "ws",
            DanmakuTransportMode.Wss => "wss"
        };

        return new DanmuInfo
        {
            Url = new UriBuilder(scheme, result.Host, result.Port, "/sub").Uri,
            Transport = result.TransportMode,
            Token = result.Token
        };
    }

    public struct DanmakuServerInfo
    {
        public DanmakuTransportMode TransportMode;
        public string Host;
        public int Port;
        public string Token;
    }
}
