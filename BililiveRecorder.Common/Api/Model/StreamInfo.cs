namespace BililiveRecorder.Common.Api.Model;


public enum StreamCodec
{
    H264,
    H265
}

public class StreamInfo
{

    public string Url { get; set; } = string.Empty;
    public int Qn;
    public string QnName { get; set; } = string.Empty;
}
