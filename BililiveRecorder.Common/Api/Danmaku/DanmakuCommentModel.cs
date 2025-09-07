#nullable enable
using Newtonsoft.Json.Linq;

namespace BililiveRecorder.Common.Api.Danmaku;

// public enum DanmakuMsgType
// {
//     /// <summary>
//     /// 彈幕
//     /// </summary>
//     Comment,
//     /// <summary>
//     /// 禮物
//     /// </summary>
//     GiftSend,
//     /// <summary>
//     /// 直播開始
//     /// </summary>
//     LiveStart,
//     /// <summary>
//     /// 直播結束
//     /// </summary>
//     LiveEnd,
//     /// <summary>
//     /// 其他
//     /// </summary>
//     Unknown,
//     /// <summary>
//     /// 购买船票（上船）
//     /// </summary>
//     GuardBuy,
//     /// <summary>
//     /// SuperChat
//     /// </summary>
//     SuperChat,
//     /// <summary>
//     /// 房间信息更新
//     /// </summary>
//     RoomChange,
//     /// <summary>
//     /// 房间被锁定
//     /// </summary>
//     RoomLock,
//     /// <summary>
//     /// 直播被切断
//     /// </summary>
//     CutOff,
// } 

public enum DanmakuMsgType
{
    /// <summary>
    /// 彈幕
    /// </summary>
    Comment,

    /// <summary>
    /// 礼物
    /// </summary>
    GiftSend,

    /// <summary>
    /// 直播开始
    /// </summary>
    LiveStart,

    /// <summary>
    /// 直播結束
    /// </summary>
    LiveEnd,

    /// <summary>
    /// 其他
    /// </summary>
    Unknown,

    /// <summary>
    /// 购买船票（上船）
    /// </summary>
    GuardBuy,

    /// <summary>
    /// SuperChat
    /// </summary>
    SuperChat,

    /// <summary>
    /// 房间信息更新
    /// </summary>
    RoomChange,

    /// <summary>
    /// 房间被锁定
    /// </summary>
    RoomLock,

    /// <summary>
    /// 直播被切断
    /// </summary>
    CutOff,
}

public abstract class BaseDanmakeModel
{
    public JObject? RawObject { get; set; }
    /// <summary>
    /// 消息类型
    /// </summary>
    public DanmakuMsgType MsgType { get; set; }
}

public class DanmakuCommentModel : BaseDanmakeModel
{
    public string? NickName { get; set; }
    public long UserId { get; set; }
    public string? Content { get; set; }
    public int Color { get; set; } = 0xFFFFFF;
    public int FontSize { get; set; } = 16;
}

public class DanmakuGiftModel : BaseDanmakeModel
{
    public string? NickName { get; set; }
    public long UserId { get; set; }
    public string? GiftName { get; set; }
    public long GiftCount { get; set; }
}

public class DanmakuLiveEndModel : BaseDanmakeModel
{
    public DanmakuLiveEndModel()
    {
        MsgType = DanmakuMsgType.LiveEnd;
    }
}

public class DanmakuLiveStartModel : BaseDanmakeModel
{
    public DanmakuLiveStartModel()
    {
        MsgType = DanmakuMsgType.LiveStart;
    }
}
