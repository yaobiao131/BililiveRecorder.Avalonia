using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Common.Scripting;
using BililiveRecorder.Core;
using BililiveRecorder.Core.Recording;
using BililiveRecorder.Flv;
using Microsoft.Extensions.DependencyInjection;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRecorderConfig(this IServiceCollection services, ConfigV3 config) => services
        .AddSingleton(config)
        .AddSingleton(sp => sp.GetRequiredService<ConfigV3>().Global);

    public static IServiceCollection AddRecorder(this IServiceCollection services) => services
        .AddSingleton<IMemoryStreamProvider, RecyclableMemoryStreamProvider>()
        .AddRecorderPollyPolicy()
        .AddRecorderApiClients()
        .AddRecorderRecording()
        .AddSingleton<IRecorder, Recorder>()
        .AddSingleton<IRoomFactory, RoomFactory>()
        .AddSingleton<UserScriptRunner>();

    private static IServiceCollection AddRecorderPollyPolicy(this IServiceCollection services) => services
        .AddBiliBiliRecorderPollyPolicy();

    public static IServiceCollection AddRecorderApiClients(this IServiceCollection services) => services
        .AddBiliBiliRecorderApiClients()
        .AddDouyinRecorderApiClients()
        .AddDouyuRecorderApiClients()
        .AddHuyaRecorderApiClients();

    public static IServiceCollection AddRecorderRecording(this IServiceCollection services) => services
        .AddScoped<IRecordTaskFactory, RecordTaskFactory>()
        .AddScoped<IFlvProcessingContextWriterFactory, FlvProcessingContextWriterWithFileWriterFactory>()
        .AddScoped<IFlvTagReaderFactory, FlvTagReaderFactory>()
        .AddScoped<ITagGroupReaderFactory, TagGroupReaderFactory>();
}
