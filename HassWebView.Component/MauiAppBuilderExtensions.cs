using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using HassWebView.Component.Models;

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
        return builder.UseHassComponents(options => { });
    }

    /// <summary>
    /// Registers the HassWebView component library with custom configuration.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="configure">Action to configure HassComponent options.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static MauiAppBuilder UseHassComponents(this MauiAppBuilder builder, Action<HassComponentOptions> configure)
    {
        var options = new HassComponentOptions();
        configure?.Invoke(options);

        // 设置默认尺寸
        if (options.DefaultSize.HasValue)
        {
            SizeManager.SetSize(options.DefaultSize.Value);
        }

        builder.Services.AddSingleton<IMauiInitializeService, HassComponentInitializer>();
        return builder;
    }
}

/// <summary>
/// Configuration options for HassWebView components.
/// </summary>
public class HassComponentOptions
{
    /// <summary>
    /// Gets or sets the default component size. If null, defaults to ComponentSize.Phone.
    /// </summary>
    public ComponentSize? DefaultSize { get; set; }
}