using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Danmaku;
using BililiveRecorder.Douyin;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.V8;
using Microsoft.Extensions.DependencyInjection;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDouyinRecorderApiClients(this IServiceCollection services) => services
        .AddJavascript()
        .AddSingleton<DouyinHttpApiClient>()
        .AddKeyedSingleton<ICookieTester>(Platform.Douyin, (sp, obj) => sp.GetRequiredService<DouyinHttpApiClient>())
        // .AddSingleton<BiliBiliPolicyWrappedApiClient<DouyinHttpApiClient>>()
        .AddKeyedSingleton<IApiClient>(Platform.Douyin, (sp, obj) => sp.GetRequiredService<DouyinHttpApiClient>())
        .AddKeyedSingleton<IDanmakuServerApiClient>(Platform.Douyin, (sp, obj) => sp.GetRequiredService<DouyinHttpApiClient>())
        .AddKeyedScoped<IDanmakuClient, DouyinDanmakuClient>(Platform.Douyin)
        .AddKeyedScoped<IBasicDanmakuWriter, BasicDanmakuWriter>(Platform.Douyin);

    private static IServiceCollection AddJavascript(this IServiceCollection services)
    {
        services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();
        return services;
    }
}
