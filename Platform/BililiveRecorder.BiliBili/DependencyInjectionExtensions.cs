using BililiveRecorder.BiliBili;
using BililiveRecorder.BiliBili.Danmaku;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Danmaku;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBiliBiliRecorderPollyPolicy(this IServiceCollection services) => services
        .AddScoped<BiliBiliPollyPolicy>()
        .AddKeyedSingleton<IReadOnlyPolicyRegistry<string>>(
            Platform.BiliBili,
            (sp, o) => sp.GetRequiredService<BiliBiliPollyPolicy>()
        );


    public static IServiceCollection AddBiliBiliRecorderApiClients(this IServiceCollection services) => services
        .AddSingleton<BiliBiliHttpApiClient>()
        .AddKeyedSingleton<ICookieTester>(
            Platform.BiliBili,
            (sp, obj) => sp.GetRequiredService<BiliBiliHttpApiClient>()
        )
        .AddSingleton<BiliBiliPolicyWrappedApiClient<BiliBiliHttpApiClient>>()
        .AddKeyedSingleton<IApiClient>(Platform.BiliBili, (sp, obj) => sp.GetRequiredService<BiliBiliPolicyWrappedApiClient<BiliBiliHttpApiClient>>())
        .AddKeyedSingleton<IDanmakuServerApiClient>(Platform.BiliBili, (sp, obj) => sp.GetRequiredService<BiliBiliPolicyWrappedApiClient<BiliBiliHttpApiClient>>())
        .AddKeyedScoped<IDanmakuClient, BiliBiliDanmakuClient>(Platform.BiliBili)
        .AddKeyedScoped<IBasicDanmakuWriter, BiliBiliDanmakuWriter>(Platform.BiliBili);
}
