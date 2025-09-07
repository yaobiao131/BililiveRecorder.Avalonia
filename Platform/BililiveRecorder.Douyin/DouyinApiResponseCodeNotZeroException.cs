namespace BililiveRecorder.Douyin;

internal class DouyinApiResponseCodeNotZeroException(int? code, string? body) : Exception(message: "Douyin API Code: " + (code?.ToString() ?? "(null)") + "\n" + body)
{
    public int? Code { get; } = code;
    public string? Body { get; } = body;
}
