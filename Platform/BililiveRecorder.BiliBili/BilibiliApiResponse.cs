using Newtonsoft.Json;

namespace BililiveRecorder.BiliBili;

internal class BilibiliApiResponse<T> where T : class
{
    [JsonProperty("code")]
    public int? Code { get; set; }

    [JsonProperty("data")]
    public T? Data { get; set; }
}
