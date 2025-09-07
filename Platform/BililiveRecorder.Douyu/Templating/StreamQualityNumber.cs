using System.Runtime.CompilerServices;

namespace BililiveRecorder.Douyu.Templating;
internal static class StreamQualityNumber
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MapToString(int qn) => qn switch
    {
        0 => "原画",
        2 => "高清",
        -1 => "录播姬脚本",
        _ => $"未知({qn})"
    };
}
