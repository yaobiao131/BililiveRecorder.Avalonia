using Newtonsoft.Json.Linq;

namespace BililiveRecorder.Common.Api.Model;

public class RoomInfo
{
    public InnerRoomInfo Room { get; set; } = new();
    public InnerUserInfo User { get; set; } = new();

    public JObject? RawApiJsonData;

    public class InnerUserInfo
    {
        public string Name { get; set; } = string.Empty;
    }

    public class InnerRoomInfo
    {
        public long Uid { get; set; }

        public long RoomId { get; set; }

        public int ShortId { get; set; }

        public int LiveStatus { get; set; }

        public int AreaId { get; set; }
        public int ParentAreaId { get; set; }

        public string AreaName { get; set; } = string.Empty;
        public string ParentAreaName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
    }
}
