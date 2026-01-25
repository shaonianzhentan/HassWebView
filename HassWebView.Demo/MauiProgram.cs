using HassWebView.Core;
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
                .UseHassWebView()
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
            //builder.Services.AddTransient<MediaPage>();
            //builder.Services.AddTransient<WebPage>();

            return builder.Build();
        }
    }
}
