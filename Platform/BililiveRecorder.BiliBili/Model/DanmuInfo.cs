using BililiveRecorder.Common.Api.Model;
using Newtonsoft.Json;

namespace BililiveRecorder.BiliBili.Model;

internal class DanmuInfo: BaseDanmuInfo
{
    [JsonProperty("host_list")] public HostListItem[] HostList { get; set; } = [];

    [JsonProperty("token")] public string Token { get; set; } = string.Empty;

    public class HostListItem
    {
        [JsonProperty("host")] public string Host { get; set; } = string.Empty;

        [JsonProperty("port")] public int Port { get; set; }

        [JsonProperty("ws_port")] public int WsPort { get; set; }

        [JsonProperty("wss_port")] public int WssPort { get; set; }
    }
}
