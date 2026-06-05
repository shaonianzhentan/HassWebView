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

        // 初始化尺寸管理器（会自动从存储加载）
        var defaultSize = options.DefaultSize ?? ComponentSize.Phone;
        SizeManager.Initialize(defaultSize);

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

        // 注册初始化服务，传递配置选项
        builder.Services.AddSingleton(options);
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

    /// <summary>
    /// Gets or sets the default theme mode. If null, defaults to ThemeMode.Dark.
    /// </summary>
    public ThemeMode? DefaultTheme { get; set; }
}