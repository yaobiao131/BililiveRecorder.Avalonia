using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Api.Model;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

namespace BililiveRecorder.BiliBili;

internal class BiliBiliPolicyWrappedApiClient<T> : IApiClient, IBiliBiliDanmakuServerApiClient, IDisposable
    where T : class, IApiClient, IBiliBiliDanmakuServerApiClient, IDisposable
{
    private readonly T client;
    private readonly IReadOnlyPolicyRegistry<string> policies;

    public Dictionary<string, string> DanmakuHeaders => client.DanmakuHeaders;
    public Dictionary<string, string> Headers => client.Headers;

    public BiliBiliPolicyWrappedApiClient(T client, IServiceProvider sp)
    {
        var policies = sp.GetKeyedService<IReadOnlyPolicyRegistry<string>>(Platform.BiliBili);
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.policies = policies ?? throw new ArgumentNullException(nameof(policies));
    }

    public long GetUid() => client.GetUid();
    public string? GetBuvid3() => client.GetBuvid3();

    public async Task<BaseDanmuInfo> GetDanmakuServerAsync(long roomid) => await policies
        .Get<IAsyncPolicy>(PolicyNames.PolicyDanmakuApiRequestAsync)
        .ExecuteAsync(_ => client.GetDanmakuServerAsync(roomid), new Context(PolicyNames.CacheKeyDanmaku + ":" + roomid))
        .ConfigureAwait(false);

    public async Task<RoomInfo> GetRoomInfoAsync(long roomid) => await policies
        .Get<IAsyncPolicy>(PolicyNames.PolicyRoomInfoApiRequestAsync)
        .ExecuteAsync(_ => client.GetRoomInfoAsync(roomid), new Context(PolicyNames.CacheKeyRoomInfo + ":" + roomid))
        .ConfigureAwait(false);

    public async Task<StreamInfo> GetStreamUrlAsync(long roomid, string? allowedQn) => await policies
        .Get<IAsyncPolicy>(PolicyNames.PolicyStreamApiRequestAsync)
        .ExecuteAsync(_ => client.GetStreamUrlAsync(roomid, allowedQn), new Context(PolicyNames.CacheKeyStream + ":" + roomid))
        .ConfigureAwait(false);

    public void Dispose() => client.Dispose();
}
