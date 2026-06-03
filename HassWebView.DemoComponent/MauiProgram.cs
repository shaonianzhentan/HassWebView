using HassWebView.Component;
using HassWebView.DemoComponent.Views;
using Microsoft.Extensions.Logging;

namespace HassWebView.DemoComponent;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        try
        {
            builder
                .UseMauiApp<App>()
                // 使用默认配置（默认尺寸为 Phone）
                .UseHassComponents()
                // 或者自定义默认尺寸：
                // .UseHassComponents(options => options.DefaultSize = HassWebView.Component.Models.ComponentSize.Tablet)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MauiApp CreateMauiApp failed: {ex}");
            throw;
        }
    }
}