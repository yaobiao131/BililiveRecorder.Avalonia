using BililiveRecorder.Avalonia;
using BililiveRecorder.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BililiveRecorder.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDispatchProvider(this IServiceCollection services) =>
        services.AddSingleton<IDispatchProvider, AvaloniaDispatchProvider>();
}
