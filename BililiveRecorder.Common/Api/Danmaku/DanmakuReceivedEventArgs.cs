namespace BililiveRecorder.Common.Api.Danmaku
{
    public class DanmakuReceivedEventArgs(BaseDanmakeModel danmaku) : EventArgs
    {
        public readonly BaseDanmakeModel Danmaku = danmaku ?? throw new ArgumentNullException(nameof(danmaku));
    }
}
