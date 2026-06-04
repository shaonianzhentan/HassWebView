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

        // 设置默认尺寸或启用自适应
        if (options.AutoDetectSize)
        {
            // 延迟检测屏幕尺寸
            Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                try
                {
                    var window = Application.Current?.Windows[0];
                    if (window != null)
                    {
                        var width = window.Width;
                        var size = width switch
                        {
                            > 1200 => ComponentSize.TV,
                            > 768 => ComponentSize.Tablet,
                            _ => ComponentSize.Phone
                        };
                        SizeManager.SetSize(size);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UseHassComponents] Auto-detect size failed: {ex.Message}");
                }
            });
        }
        else if (options.DefaultSize.HasValue)
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
    /// Ignored if AutoDetectSize is true.
    /// </summary>
    public ComponentSize? DefaultSize { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically detect screen size and set component size accordingly.
    /// Default is false.
    /// </summary>
    public bool AutoDetectSize { get; set; }
}