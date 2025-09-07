using Newtonsoft.Json;

namespace BIliliveRecorder.Huya.Model;

internal class HuyaApiResponse<T>
{
    [JsonProperty("status")] internal int Status { get; set; }
    [JsonProperty("message")] internal string Message { get; set; } = string.Empty;
    [JsonProperty("data")] internal T? Data { get; set; }
}
