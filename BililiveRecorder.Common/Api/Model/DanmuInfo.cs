using BililiveRecorder.Common.Config;

namespace BililiveRecorder.Common.Api.Model;

public class BaseDanmuInfo
{
    public required Uri Url { get; set; }
    public DanmakuTransportMode Transport { get; set; }

    public List<byte[]> RegMessage { get; set; } = [];
}
