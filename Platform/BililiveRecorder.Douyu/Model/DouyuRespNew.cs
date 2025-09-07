using BililiveRecorder.Common.Api.Model;
using Newtonsoft.Json;

namespace BililiveRecorder.Douyu.Model;

internal class DouyuRespNew
{
    [JsonProperty("room")] internal InnerRoom Room { get; set; } = new();
    [JsonProperty("column")] internal InnerColumn Column { get; set; } = new();
    [JsonProperty("child_cate")] internal InnerChildCate ChildCate { get; set; } = new();

    internal RoomInfo ToRoomInfo()
    {
        return new RoomInfo
        {
            Room = new RoomInfo.InnerRoomInfo
            {
                Title = Room.RoomName,
                RoomId = Room.RoomId,
                ShortId = Room.VipId,
                LiveStatus = Room is { ShowStatus: 1, VideoLoop: 0 } ? 1 : 0,
                ParentAreaName = Column.CateName,
                AreaName = ChildCate.Name
            },
            User = new RoomInfo.InnerUserInfo
            {
                Name = Room.Nickname
            }
        };
    }

    internal class InnerRoom
    {
        [JsonProperty("nickname")] internal string Nickname { get; set; } = string.Empty;
        [JsonProperty("vipId")] internal int VipId { get; set; }
        [JsonProperty("room_id")] internal long RoomId { get; set; }
        [JsonProperty("room_name")] internal string RoomName { get; set; } = string.Empty;
        [JsonProperty("show_status")] internal int ShowStatus { get; set; }
        [JsonProperty("videoLoop")] internal int VideoLoop { get; set; }
    }

    internal class InnerColumn
    {
        [JsonProperty("cate_name")] internal string CateName { get; set; } = string.Empty;
    }

    internal class InnerChildCate
    {
        [JsonProperty("name")] internal string Name { get; set; } = string.Empty;
    }
}
