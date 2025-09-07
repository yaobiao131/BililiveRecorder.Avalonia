using System.Text.RegularExpressions;

namespace BililiveRecorder.Core;

public static partial class RoomIdFromUrl
{
    [GeneratedRegex("""^(?:(?:https?:\/\/)?live\.bilibili\.com\/(?:blanc\/|h5\/)?)?(\d+)\/?(?:[#\?].*)?$""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "zh-CN")]
    public static partial Regex BiliBiliRegex();

    [GeneratedRegex("""live\.douyin\.com/(\d+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "zh-CN")]
    public static partial Regex DouyinRegex();

    [GeneratedRegex("""douyu\.com/(\d+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "zh-CN")]
    public static partial Regex DouyuRegex();

    [GeneratedRegex("""huya\.com/(\d+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "zh-CN")]
    public static partial Regex HuyaRegex();
}
