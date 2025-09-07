using BililiveRecorder.Common.Api.Danmaku;

namespace BililiveRecorder.Common.Danmaku;

public interface IBasicDanmakuWriter : IDisposable
{
    void Disable();
    void EnableWithPath(string path, IRoom room);
    Task WriteAsync(BaseDanmakeModel danmakuModel);
}
