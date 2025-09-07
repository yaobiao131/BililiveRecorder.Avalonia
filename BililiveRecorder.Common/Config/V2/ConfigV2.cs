#nullable enable
using Newtonsoft.Json;

namespace BililiveRecorder.Common.Config.V2
{
    [Obsolete("Use Config v3")]
    internal class ConfigV2 : ConfigBase
    {
        public override int Version => 2;

        [JsonProperty("global")]
        public Common.Config.V2.GlobalConfig Global { get; set; } = new Common.Config.V2.GlobalConfig();

        [JsonProperty("rooms")]
        public List<Common.Config.V2.RoomConfig> Rooms { get; set; } = new List<Common.Config.V2.RoomConfig>();

        [JsonIgnore]
        public bool DisableConfigSave { get; set; } = false; // for CLI
    }

    [Obsolete("Use Config v3")]
    internal partial class RoomConfig
    {
        public RoomConfig() : base(x => x.AutoMap(p => new[] { "Has" + p.Name }))
        { }

        internal void SetParent(Common.Config.V2.GlobalConfig? config) => this.Parent = config;

        public string? WorkDirectory => this.GetPropertyValue<string>();
    }

    [Obsolete("Use Config v3")]
    internal partial class GlobalConfig
    {
        public GlobalConfig() : base(x => x.AutoMap(p => new[] { "Has" + p.Name }))
        {
            this.Parent = DefaultConfig.Instance;
        }

        /// <summary>
        /// 当前工作目录
        /// </summary>
        public string? WorkDirectory
        {
            get => this.GetPropertyValue<string>();
            set => this.SetPropertyValue(value);
        }
    }
}
