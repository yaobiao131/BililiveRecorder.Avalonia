using Newtonsoft.Json;

namespace BililiveRecorder.Douyu.Model;

internal class DouyuPlayInfo
{
    [JsonProperty("rtmp_cdn")] internal string RtmpCdn { get; set; } = string.Empty;
    [JsonProperty("rtmp_url")] internal string RtmpUrl { get; set; } = string.Empty;
    [JsonProperty("rtmp_live")] internal string RtmpLive { get; set; } = string.Empty;
    [JsonProperty("rate")] internal int Rate { get; set; }
}
