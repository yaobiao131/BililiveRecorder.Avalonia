#nullable enable
using HierarchicalPropertyDefault;
using Newtonsoft.Json;

namespace BililiveRecorder.Common.Config.V3
{
    public sealed class ConfigV3 : ConfigBase
    {
        public override int Version => 3;

        [JsonProperty("global")] public GlobalConfig Global { get; set; } = new();

        [JsonProperty("rooms")] public List<RoomConfig> Rooms { get; set; } = [];

        // for CLI
        [JsonIgnore] public bool DisableConfigSave { get; set; } = false;

        // for CLI
        [JsonIgnore] public string? ConfigPathOverride { get; set; }
    }

    public partial class RoomConfig() : HierarchicalObject<GlobalConfig, RoomConfig>(x => x.AutoMap(p => new[] { "Has" + p.Name })), IFileNameConfig
    {
        public void SetParent(GlobalConfig? config) => Parent = config;

        public string? WorkDirectory => GetPropertyValue<string>();
    }

    public partial class GlobalConfig : IFileNameConfig
    {
        public GlobalConfig() : base(x => x.AutoMap(p => new[] { "Has" + p.Name }))
        {
            Parent = DefaultConfig.Instance;
        }

        /// <summary>
        /// 当前工作目录
        /// </summary>
        public string? WorkDirectory
        {
            get => GetPropertyValue<string>();
            set => SetPropertyValue(value);
        }
    }
}
