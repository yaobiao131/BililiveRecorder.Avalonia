using BililiveRecorder.Common.Api.Danmaku;
using Newtonsoft.Json.Linq;

namespace BililiveRecorder.BiliBili;

public class BiliBiliDanmakuModel : BaseDanmakeModel
{
    /// <summary>
    /// 房间标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 大分区
    /// </summary>
    public string? ParentAreaName { get; set; }

    /// <summary>
    /// 子分区
    /// </summary>
    public string? AreaName { get; set; }

    /// <summary>
    /// 彈幕內容
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// </list></para>
    /// </summary>
    public string? CommentText { get; set; }

    /// <summary>
    /// 消息触发者用户名
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// <item><see cref="DanmakuMsgType.GiftSend"/></item>
    /// <item><see cref="DanmakuMsgType.Welcome"/></item>
    /// <item><see cref="DanmakuMsgType.WelcomeGuard"/></item>
    /// <item><see cref="DanmakuMsgType.GuardBuy"/></item>
    /// </list></para>
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// SC 价格
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// SC 保持时间
    /// </summary>
    public int SCKeepTime { get; set; }

    /// <summary>
    /// 消息触发者用户ID
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// <item><see cref="DanmakuMsgType.GiftSend"/></item>
    /// <item><see cref="DanmakuMsgType.Welcome"/></item>
    /// <item><see cref="DanmakuMsgType.WelcomeGuard"/></item>
    /// <item><see cref="DanmakuMsgType.GuardBuy"/></item>
    /// </list></para>
    /// </summary>
    public long UserID { get; set; }

    /// <summary>
    /// 用户舰队等级
    /// <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// <item><see cref="DanmakuMsgType.WelcomeGuard"/></item>
    /// <item><see cref="DanmakuMsgType.GuardBuy"/></item>
    /// </list></para>
    /// </summary>
    public int UserGuardLevel { get; set; }

    /// <summary>
    /// 禮物名稱
    /// </summary>
    public string? GiftName { get; set; }

    /// <summary>
    /// 礼物数量
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.GiftSend"/></item>
    /// <item><see cref="DanmakuMsgType.GuardBuy"/></item>
    /// </list></para>
    /// <para>此字段也用于标识上船 <see cref="DanmakuMsgType.GuardBuy"/> 的数量（月数）</para>
    /// </summary>
    public int GiftCount { get; set; }

    /// <summary>
    /// 该用户是否为房管（包括主播）
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// <item><see cref="DanmakuMsgType.GiftSend"/></item>
    /// </list></para>
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// 是否VIP用戶(老爺)
    /// <para>此项有值的消息类型：<list type="bullet">
    /// <item><see cref="DanmakuMsgType.Comment"/></item>
    /// <item><see cref="DanmakuMsgType.Welcome"/></item>
    /// </list></para>
    /// </summary>
    public bool IsVIP { get; set; }

    /// <summary>
    /// <see cref="DanmakuMsgType.LiveStart"/>,<see cref="DanmakuMsgType.LiveEnd"/> 事件对应的房间号
    /// </summary>
    public string? RoomID { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    public string RawString { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    public JObject? RawObject { get; set; }

    private BiliBiliDanmakuModel()
    {
        RawString = string.Empty;
    }

    public BiliBiliDanmakuModel(string json)
    {
        RawString = json;

        var obj = JObject.Parse(json);
        RawObject = obj;

        var cmd = obj["cmd"]?.ToObject<string>();

        if (cmd?.StartsWith("DANMU_MSG:") ?? false)
            cmd = "DANMU_MSG";

        switch (cmd)
        {
            case "LIVE": // 开播
                MsgType = DanmakuMsgType.LiveStart;
                RoomID = obj["roomid"]?.ToObject<string>();
                break;
            case "PREPARING": // 下播
                MsgType = DanmakuMsgType.LiveEnd;
                RoomID = obj["roomid"]?.ToObject<string>();
                break;
            case "DANMU_MSG": // 弹幕
                MsgType = DanmakuMsgType.Comment;
                CommentText = obj["info"]?[1]?.ToObject<string>();
                UserID = obj["info"]?[2]?[0]?.ToObject<long>() ?? 0;
                UserName = obj["info"]?[2]?[1]?.ToObject<string>();
                IsAdmin = obj["info"]?[2]?[2]?.ToObject<string>() == "1";
                IsVIP = obj["info"]?[2]?[3]?.ToObject<string>() == "1";
                UserGuardLevel = obj["info"]?[7]?.ToObject<int>() ?? 0;
                break;
            case "SEND_GIFT": // 送礼物
                MsgType = DanmakuMsgType.GiftSend;
                GiftName = obj["data"]?["giftName"]?.ToObject<string>();
                UserName = obj["data"]?["uname"]?.ToObject<string>();
                UserID = obj["data"]?["uid"]?.ToObject<long>() ?? 0;
                GiftCount = obj["data"]?["num"]?.ToObject<int>() ?? 0;
                break;
            case "GUARD_BUY": // 购买舰长
            {
                MsgType = DanmakuMsgType.GuardBuy;
                UserID = obj["data"]?["uid"]?.ToObject<long>() ?? 0;
                UserName = obj["data"]?["username"]?.ToObject<string>();
                UserGuardLevel = obj["data"]?["guard_level"]?.ToObject<int>() ?? 0;
                GiftName = UserGuardLevel == 3 ? "舰长" : UserGuardLevel == 2 ? "提督" : UserGuardLevel == 1 ? "总督" : "";
                GiftCount = obj["data"]?["num"]?.ToObject<int>() ?? 0;
                break;
            }
            case "SUPER_CHAT_MESSAGE": // SC
            {
                MsgType = DanmakuMsgType.SuperChat;
                CommentText = obj["data"]?["message"]?.ToString();
                UserID = obj["data"]?["uid"]?.ToObject<long>() ?? 0;
                UserName = obj["data"]?["user_info"]?["uname"]?.ToString();
                Price = obj["data"]?["price"]?.ToObject<double>() ?? 0;
                SCKeepTime = obj["data"]?["time"]?.ToObject<int>() ?? 0;
                break;
            }
            case "ROOM_CHANGE": // 房间信息变更
            {
                MsgType = DanmakuMsgType.RoomChange;
                Title = obj["data"]?["title"]?.ToObject<string>();
                AreaName = obj["data"]?["area_name"]?.ToObject<string>();
                ParentAreaName = obj["data"]?["parent_area_name"]?.ToObject<string>();
                break;
            }
            case "ROOM_LOCK": // 房间被锁定
            {
                MsgType = DanmakuMsgType.RoomLock;
                break;
            }
            case "CUT_OFF": // 直播被切断
            {
                MsgType = DanmakuMsgType.CutOff;
                break;
            }
            default:
            {
                MsgType = DanmakuMsgType.Unknown;
                break;
            }
        }
    }
}
