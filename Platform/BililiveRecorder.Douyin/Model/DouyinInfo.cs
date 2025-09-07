using BililiveRecorder.Common;
using BililiveRecorder.Common.Api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BililiveRecorder.Douyin.Model;

internal class DouyinInfo
{
    [JsonProperty("room_status")] internal int RoomStatus;

    [JsonProperty("data")] internal DouyinData[] Data = [];

    [JsonProperty("partition_road_map")] internal InnerPartitionRoadMap? PartitionRoadMap;

    [JsonProperty("user")] internal DouyinUserInfo UserInfo = new();

    internal RoomInfo ToRoomInfo(long roomId)
    {
        return new RoomInfo
        {
            RawApiJsonData = JObject.FromObject(this),
            User = new RoomInfo.InnerUserInfo
            {
                Name = UserInfo.Nickname
            },
            Room = new RoomInfo.InnerRoomInfo
            {
                Title = Data[0].Title,
                RoomId = roomId,
                LiveStatus = RoomStatus switch
                {
                    0 => 1,
                    _ => 0
                },
                ParentAreaName = PartitionRoadMap?.Partition.Title ?? string.Empty,
                AreaName = PartitionRoadMap?.SubPartition.Partition.Title ?? string.Empty
            }
        };
    }

    internal class DouyinUserInfo
    {
        [JsonProperty("nickname")] internal string Nickname = string.Empty;
    }

    internal class DouyinData
    {
        [JsonProperty("title")] internal string Title = string.Empty;
        [JsonProperty("id_str")] internal string IdStr = string.Empty;
        [JsonProperty("stream_url")] public DouyinStream? StreamUrl;
    }

    internal class DouyinStream
    {
        [JsonProperty("live_core_sdk_data")] internal InnerLiveCoreSdkData LiveCoreSdkData = new();

        internal class InnerLiveCoreSdkData
        {
            [JsonProperty("pull_data")] internal InnerPullData PullData = new();

            internal class InnerPullData
            {
                [JsonProperty("options")] internal InnerOption Options = new();

                [JsonConverter(typeof(NestedStringConverter<InnerStreamData>))] [JsonProperty("stream_data")]
                internal InnerStreamData StreamData = new();

                internal class InnerOption
                {
                    [JsonProperty("qualities")] internal List<InnerQuality> Qualities = [];

                    internal class InnerQuality
                    {
                        [JsonProperty("name")] internal string Name = string.Empty;
                        [JsonProperty("sdk_key")] internal string SdkKey = string.Empty;
                        [JsonProperty("v_codec")] internal string VCodec = string.Empty;
                        [JsonProperty("resolution")] internal string Resolution = string.Empty;
                        [JsonProperty("level")] internal int Level;
                        [JsonProperty("v_bit_rate")] internal int VBitRate;
                        [JsonProperty("disable")] internal int Disable;
                    }
                }

                internal class InnerStreamData
                {
                    [JsonProperty("data")] internal Dictionary<string, InnerMain> Data = new();

                    internal class InnerMain
                    {
                        [JsonProperty("main")] internal InnerStream Main = new();

                        internal class InnerStream
                        {
                            [JsonProperty("flv")] internal string Flv = string.Empty;
                            [JsonProperty("hls")] internal string Hls = string.Empty;
                        }
                    }
                }
            }
        }
    }


    internal class InnerPartitionRoadMap
    {
        [JsonProperty("partition")] internal InnerPartition Partition = new();
        [JsonProperty("sub_partition")] internal InnerSubPartition SubPartition = new();

        internal class InnerPartition
        {
            [JsonProperty("title")] internal string Title = string.Empty;
        }

        internal class InnerSubPartition
        {
            [JsonProperty("partition")] internal InnerPartition Partition = new();
        }
    }
}
