using BililiveRecorder.Common;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Danmaku;
using BililiveRecorder.Douyu;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.V8;
using Microsoft.Extensions.DependencyInjection;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDouyuRecorderApiClients(this IServiceCollection services) => services
        .AddJavascript()
        .AddSingleton<DouyuHttpApiClient>()
        .AddKeyedSingleton<IApiClient>(Platform.Douyu, (sp, obj) => sp.GetRequiredService<DouyuHttpApiClient>())
        .AddKeyedSingleton<IDanmakuServerApiClient>(Platform.Douyu, (sp, obj) => sp.GetRequiredService<DouyuHttpApiClient>())
        .AddKeyedScoped<IDanmakuClient, DouyuDanmakuClient>(Platform.Douyu)
        .AddKeyedScoped<IBasicDanmakuWriter, BasicDanmakuWriter>(Platform.Douyu);


    private static IServiceCollection AddJavascript(this IServiceCollection services)
    {
        services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();
        return services;
    }
}
