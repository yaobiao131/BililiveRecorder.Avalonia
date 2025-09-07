using BililiveRecorder.Common.Api;

namespace BililiveRecorder.BiliBili;

public interface IBiliBiliDanmakuServerApiClient: IDanmakuServerApiClient
{
    long GetUid();
    string? GetBuvid3();
}
