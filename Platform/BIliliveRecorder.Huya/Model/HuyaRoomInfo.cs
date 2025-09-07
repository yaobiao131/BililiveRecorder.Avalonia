using BililiveRecorder.Common;
using BililiveRecorder.Common.Api.Model;
using Newtonsoft.Json;

namespace BIliliveRecorder.Huya.Model;

internal class HuyaRoomInfo
{
    internal InnerProfile Profile { get; set; } = new();
    internal InnerPlayerConfig PlayerConfig { get; set; } = new();
    [JsonProperty("stream")] internal InnerStream? Steam { get; set; }
    [JsonProperty("liveStatus")] internal string LiveStatus { get; set; } = string.Empty;
    [JsonProperty("liveData")] internal InnerLiveData LiveData { get; set; } = new();
    [JsonProperty("chTopId")] internal long ChTopId { get; set; }
    [JsonProperty("subChId")] internal long SubChId { get; set; }

    internal RoomInfo ToRoomInfo()
    {
        return new RoomInfo
        {
            Room = new RoomInfo.InnerRoomInfo
            {
                RoomId = Profile.ProfileRoom,
                Title = Profile.Introduction,
                ParentAreaName = Profile.GameFullName,
                LiveStatus = Profile.State == "ON" ? 1 : 0
            },
            User = new RoomInfo.InnerUserInfo
            {
                Name = PlayerConfig.Stream.Data[0].GameLiveInfo.Nick
            }
        };
    }

    internal class InnerProfile
    {
        [JsonProperty("type")] internal string Type { get; set; } = string.Empty;
        [JsonProperty("state")] internal string State { get; set; } = "OFF";
        [JsonProperty("isOn")] public bool IsOn { get; set; }
        [JsonProperty("isOff")] public bool IsOff { get; set; }
        [JsonProperty("isReplay")] public bool IsReplay { get; set; }
        [JsonProperty("isPayRoom")] public int IsPayRoom { get; set; }
        [JsonProperty("isSecret")] public int IsSecret { get; set; }
        [JsonProperty("roomPayPassword")] public string RoomPayPassword { get; set; } = string.Empty;
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("sid")] public long Sid { get; set; }
        [JsonProperty("channel")] public long Channel { get; set; }
        [JsonProperty("liveChannel")] public long LiveChannel { get; set; }
        [JsonProperty("liveId")] public string LiveId { get; set; } = string.Empty;
        [JsonProperty("shortChannel")] public int ShortChannel { get; set; }
        [JsonProperty("isBluRay")] public int IsBluRay { get; set; }
        [JsonProperty("gameFullName")] public string GameFullName { get; set; } = string.Empty;
        [JsonProperty("gameHostName")] public string GameHostName { get; set; } = string.Empty;
        [JsonProperty("screenType")] public int ScreenType { get; set; }
        [JsonProperty("startTime")] public int StartTime { get; set; }
        [JsonProperty("totalCount")] public int TotalCount { get; set; }
        [JsonProperty("cameraOpen")] public int CameraOpen { get; set; }
        [JsonProperty("liveCompatibleFlag")] public int LiveCompatibleFlag { get; set; }
        [JsonProperty("bussType")] public int BussType { get; set; }
        [JsonProperty("isPlatinum")] public int IsPlatinum { get; set; }
        [JsonProperty("isAutoBitrate")] public int IsAutoBitrate { get; set; }
        [JsonProperty("screenshot")] public string Screenshot { get; set; } = string.Empty;
        [JsonProperty("previewUrl")] public string PreviewUrl { get; set; } = string.Empty;
        [JsonProperty("gameId")] public int GameId { get; set; }
        [JsonProperty("liveSourceType")] public int LiveSourceType { get; set; }
        [JsonProperty("privateHost")] public string PrivateHost { get; set; } = string.Empty;
        [JsonProperty("profileRoom")] public int ProfileRoom { get; set; }
        [JsonProperty("recommendStatus")] public int RecommendStatus { get; set; }
        [JsonProperty("popular")] public int Popular { get; set; }
        [JsonProperty("gid")] public int Gid { get; set; }
        [JsonProperty("introduction")] public string Introduction { get; set; } = string.Empty;
        [JsonProperty("isRedirectHuya")] public int IsRedirectHuya { get; set; }
        [JsonProperty("isShowMmsProgramList")] public int IsShowMmsProgramList { get; set; }
        [JsonProperty("adStatus")] public int AdStatus { get; set; }
    }

    internal class InnerPlayerConfig
    {
        [JsonProperty("stream")] public Stream Stream { get; set; } = new();
    }

    internal class Stream
    {
        [JsonProperty("data")] public List<DataItem> Data { get; set; } = [];
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("vMultiStreamInfo")] public List<VMultiStreamInfoItem> VMultiStreamInfo { get; set; } = [];
        [JsonProperty("iWebDefaultBitRate")] public int IWebDefaultBitRate { get; set; }
        [JsonProperty("iFrameRate")] public int IFrameRate { get; set; }
    }

    internal class VMultiStreamInfoItem
    {
        [JsonProperty("sDisplayName")] public string SDisplayName { get; set; } = string.Empty;
        [JsonProperty("iBitRate")] public int IBitRate { get; set; }
        [JsonProperty("iCodecType")] public int ICodecType { get; set; }
        [JsonProperty("iCompatibleFlag")] public int ICompatibleFlag { get; set; }
        [JsonProperty("iHEVCBitRate")] public int IhevcBitRate { get; set; }
    }

    public class DataItem
    {
        [JsonProperty("gameLiveInfo")] public GameLiveInfo GameLiveInfo { get; set; } = new();
        [JsonProperty("gameStreamInfoList")] public List<GameStreamInfoListItem> GameStreamInfoList { get; set; } = [];
    }

    internal class GameStreamInfoListItem
    {
        [JsonProperty("sCdnType")] public string SCdnType { get; set; } = string.Empty;
        [JsonProperty("iIsMaster")] public int IIsMaster { get; set; }
        [JsonProperty("lChannelId")] public long LChannelId { get; set; }
        [JsonProperty("lSubChannelId")] public long LSubChannelId { get; set; }
        [JsonProperty("lPresenterUid")] public long LPresenterUid { get; set; }
        [JsonProperty("sStreamName")] public string SStreamName { get; set; } = string.Empty;
        [JsonProperty("sFlvUrl")] public string SFlvUrl { get; set; } = string.Empty;
        [JsonProperty("sFlvUrlSuffix")] public string SFlvUrlSuffix { get; set; } = string.Empty;

        [JsonProperty("sFlvAntiCode")] public string SFlvAntiCode { get; set; } = string.Empty;
        [JsonProperty("sHlsUrl")] public string SHlsUrl { get; set; } = string.Empty;

        [JsonProperty("sHlsUrlSuffix")] public string SHlsUrlSuffix { get; set; } = string.Empty;
        [JsonProperty("sHlsAntiCode")] public string SHlsAntiCode { get; set; } = string.Empty;
        [JsonProperty("iLineIndex")] public int ILineIndex { get; set; }
        [JsonProperty("iIsMultiStream")] public int IIsMultiStream { get; set; }
        [JsonProperty("iPCPriorityRate")] public int IpcPriorityRate { get; set; }
        [JsonProperty("iWebPriorityRate")] public int IWebPriorityRate { get; set; }
        [JsonProperty("iMobilePriorityRate")] public int IMobilePriorityRate { get; set; }
        [JsonProperty("iIsP2PSupport")] public int IIsP2PSupport { get; set; }
        [JsonProperty("sP2pUrl")] public string Sp2PUrl { get; set; } = string.Empty;
        [JsonProperty("sP2pUrlSuffix")] public string Sp2PUrlSuffix { get; set; } = string.Empty;
        [JsonProperty("sP2pAntiCode")] public string Sp2PAntiCode { get; set; } = string.Empty;
        [JsonProperty("lFreeFlag")] public int LFreeFlag { get; set; }
        [JsonProperty("iIsHEVCSupport")] public int IIsHevcSupport { get; set; }
        [JsonProperty("lTimespan")] public string LTimespan { get; set; } = string.Empty;
        [JsonProperty("lUpdateTime")] public int LUpdateTime { get; set; }
    }

    public class GameLiveInfo
    {
        [JsonProperty("uid")] public long Uid { get; set; }
        [JsonProperty("sex")] public int Sex { get; set; }
        [JsonProperty("gameFullName")] public string GameFullName { get; set; } = string.Empty;
        [JsonProperty("gameHostName")] public string GameHostName { get; set; } = string.Empty;
        [JsonProperty("startTime")] public int StartTime { get; set; }
        [JsonProperty("activityId")] public int ActivityId { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("totalCount")] public int TotalCount { get; set; }
        [JsonProperty("roomName")] public string RoomName { get; set; } = string.Empty;
        [JsonProperty("isSecret")] public int IsSecret { get; set; }
        [JsonProperty("cameraOpen")] public int CameraOpen { get; set; }
        [JsonProperty("liveChannel")] public long LiveChannel { get; set; }
        [JsonProperty("bussType")] public int BussType { get; set; }
        [JsonProperty("yyid")] public long Yyid { get; set; }
        [JsonProperty("screenshot")] public string Screenshot { get; set; } = string.Empty;
        [JsonProperty("activityCount")] public int ActivityCount { get; set; }
        [JsonProperty("privateHost")] public string PrivateHost { get; set; } = string.Empty;
        [JsonProperty("recommendStatus")] public int RecommendStatus { get; set; }
        [JsonProperty("nick")] public string Nick { get; set; } = string.Empty;
        [JsonProperty("shortChannel")] public int ShortChannel { get; set; }
        [JsonProperty("avatar180")] public string Avatar180 { get; set; } = string.Empty;
        [JsonProperty("gid")] public int Gid { get; set; }
        [JsonProperty("channel")] public long Channel { get; set; }
        [JsonProperty("introduction")] public string Introduction { get; set; } = string.Empty;
        [JsonProperty("profileHomeHost")] public string ProfileHomeHost { get; set; } = string.Empty;
        [JsonProperty("liveSourceType")] public int LiveSourceType { get; set; }
        [JsonProperty("screenType")] public int ScreenType { get; set; }
        [JsonProperty("bitRate")] public int BitRate { get; set; }
        [JsonProperty("gameType")] public int GameType { get; set; }
        [JsonProperty("attendeeCount")] public int AttendeeCount { get; set; }
        [JsonProperty("multiStreamFlag")] public int MultiStreamFlag { get; set; }
        [JsonProperty("codecType")] public int CodecType { get; set; }
        [JsonProperty("liveCompatibleFlag")] public int LiveCompatibleFlag { get; set; }
        [JsonProperty("profileRoom")] public int ProfileRoom { get; set; }
        [JsonProperty("liveId")] public long LiveId { get; set; }
        [JsonProperty("recommendTagName")] public string RecommendTagName { get; set; } = string.Empty;
        [JsonProperty("contentIntro")] public string ContentIntro { get; set; } = string.Empty;
    }

    internal class InnerStream
    {
        [JsonProperty("baseSteamInfoList")] internal List<InnerBaseSteamInfoList> BaseSteamInfoList { get; set; } = [];

        internal class InnerBaseSteamInfoList
        {
            [JsonProperty("sCdnType")] internal string SCdnType { get; set; } = string.Empty;
            [JsonProperty("sStreamName")] internal string SStreamName { get; set; } = string.Empty;
            [JsonProperty("sFlvUrl")] internal string SFlvUrl { get; set; } = string.Empty;
            [JsonProperty("sFlvUrlSuffix")] internal string SFlvUrlSuffix { get; set; } = string.Empty;
            [JsonProperty("sFlvAntiCode")] internal string SFlvAntiCode { get; set; } = string.Empty;
        }
    }

    internal class InnerLiveData
    {
        [JsonProperty("nick")] internal string Nick { get; set; } = string.Empty;
        [JsonProperty("yyid")] internal long Yyid { get; set; }
        [JsonProperty("channel")] internal long Channel { get; set; }
        [JsonProperty("uid")] internal long Uid { get; set; }
        [JsonProperty("introduction")] internal string Introduction { get; set; } = string.Empty;
        [JsonProperty("profileRoom")] internal int ProfileRoom { get; set; }
        [JsonProperty("gameFullName")] internal string GameFullName { get; set; } = string.Empty;

        [JsonProperty("mStreamRatioWeb")]
        [JsonConverter(typeof(NestedStringConverter<Dictionary<string, int>>))]
        internal Dictionary<string, int> MStreamRatioWeb { get; set; } = [];
    }
}
