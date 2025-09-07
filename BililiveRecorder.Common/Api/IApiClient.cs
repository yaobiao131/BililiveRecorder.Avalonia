using BililiveRecorder.Common.Api.Model;

namespace BililiveRecorder.Common.Api;

public interface IApiClient : IDisposable
{
    public Dictionary<string, string> Headers { get; }

    Task<RoomInfo?> GetRoomInfoAsync(long roomid);
    Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn = null);
}

