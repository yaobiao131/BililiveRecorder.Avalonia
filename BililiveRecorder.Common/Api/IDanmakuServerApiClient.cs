using System.Net.Http.Headers;
using BililiveRecorder.Common.Api.Model;

namespace BililiveRecorder.Common.Api
{
    public interface IDanmakuServerApiClient : IDisposable
    {
        public abstract Dictionary<string, string> DanmakuHeaders { get; }
        Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid);
    }
}
