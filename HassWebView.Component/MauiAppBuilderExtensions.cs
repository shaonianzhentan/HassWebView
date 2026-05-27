using HassWebView.Component.Services;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.Component;

public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers the HassWebView component library for use in a MAUI app.
    /// This adds the library initialization service and loads component resources.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static MauiAppBuilder UseHassComponents(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IMauiInitializeService, HassComponentInitializer>();
        builder.Services.AddSingleton(_ => AppInfoService.Instance);
        return builder;
    }
}
