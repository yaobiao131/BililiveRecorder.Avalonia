using BililiveRecorder.Common.Config.V1;
using BililiveRecorder.Common.Config.V2;
using BililiveRecorder.Common.Config.V3;
using JsonSubTypes;
using Newtonsoft.Json;

namespace BililiveRecorder.Common.Config
{
#pragma warning disable CS0618 // Type or member is obsolete
    [JsonConverter(typeof(JsonSubtypes), nameof(Version))]
    [JsonSubtypes.KnownSubType(typeof(ConfigV1Wrapper), 1)]
    [JsonSubtypes.KnownSubType(typeof(ConfigV2), 2)]
    [JsonSubtypes.KnownSubType(typeof(ConfigV3), 3)]
#pragma warning restore CS0618 // Type or member is obsolete
    public abstract class ConfigBase
    {
        [JsonProperty("$schema", Order = -2)]
        public string? DollarSignSchema { get; set; }

        [JsonProperty("version")]
        public virtual int Version { get; internal protected set; }
    }
}
