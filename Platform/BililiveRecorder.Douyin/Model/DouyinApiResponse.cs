using Newtonsoft.Json;

namespace BililiveRecorder.Douyin.Model;

internal class DouyinApiResponse<T> where T : class
{
    [JsonProperty("data")] public T? Data { get; set; }

    [JsonProperty("status_code")] public int StatusCode { get; set; }
}
