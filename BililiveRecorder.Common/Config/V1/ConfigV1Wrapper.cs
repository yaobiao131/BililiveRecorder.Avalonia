using Newtonsoft.Json;

namespace BililiveRecorder.Common.Config.V1
{
    internal sealed class ConfigV1Wrapper : ConfigBase
    {
        /// <summary>
        /// Config Data String
        /// </summary>
        [JsonProperty("data")]
        public string? Data { get; set; }
    }
}
