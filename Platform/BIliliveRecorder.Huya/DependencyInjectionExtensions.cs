using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Danmaku;
using BIliliveRecorder.Huya;
using Microsoft.Extensions.DependencyInjection;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddHuyaRecorderApiClients(this IServiceCollection services) => services
        .AddSingleton<HuyaHttpApiClient>()
        .AddKeyedSingleton<IApiClient>(Platform.Huya, (sp, obj) => sp.GetRequiredService<HuyaHttpApiClient>())
        .AddKeyedSingleton<IDanmakuServerApiClient>(Platform.Huya, (sp, obj) => sp.GetRequiredService<HuyaHttpApiClient>())
        .AddKeyedScoped<IDanmakuClient, HuyaDanmakuClient>(Platform.Huya)
        .AddKeyedScoped<IBasicDanmakuWriter, BasicDanmakuWriter>(Platform.Huya);
}
