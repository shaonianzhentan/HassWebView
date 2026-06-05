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
                // 自定义配置：平板模式 + 暗色主题
                .UseHassComponents(options =>
                {
                    options.DefaultSize = HassWebView.Component.Models.ComponentSize.Tablet;
                    options.DefaultTheme = HassWebView.Component.Models.ThemeMode.Dark;
                })
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