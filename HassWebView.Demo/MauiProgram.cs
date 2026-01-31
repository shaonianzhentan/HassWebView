using HassWebView.Core;
using HassWebView.Core.Views;
using Microsoft.Extensions.Logging;

namespace HassWebView.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                // This extension method now handles registering IHassAuthService.
                .UseHassWebView(options =>
                {
                    // 配置设置页面的导航
                    options.ShowSettingsScreen = () =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Shell.Current.GoToAsync("///MySettingsPage");
                        });
                    };

                    // 配置视频播放的导航
                    options.PlayVideo = (url) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // 用户在这里决定导航到哪个页面，例如 HassMediaPage
                            Shell.Current.GoToAsync($"{nameof(HassMediaPage)}?Url={Uri.EscapeDataString(url)}");
                        });
                    };
                })
                // This is for Android fullscreen.
                .UseImmersiveMode()
                // This extension method now handles registering KeyService and platform-specific key listeners.
                .UseRemoteControl();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Services are now registered by the extension methods above, so we can remove the explicit registrations here.

            // Register pages for dependency injection
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder.Build();
        }
    }
}
