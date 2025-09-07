using Newtonsoft.Json;

namespace BililiveRecorder.Douyu.Model;

internal class DouyuApiResponse<T>
{
    [JsonProperty("error")] internal int Error { get; set; }
    [JsonProperty("msg")] internal string Msg { get; set; } = string.Empty;

    [JsonProperty("data")] internal T? Data { get; set; }
}
