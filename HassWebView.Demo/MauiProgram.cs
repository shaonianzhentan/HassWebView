using HassWebView.Core;
using HassWebView.Core.Services;
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
                .UseHassWebView()
                .UseImmersiveMode()
                .UseRemoteControl();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register our new, refactored authentication service as a singleton
            builder.Services.AddSingleton<IHassAuthService, HassAuthService>();
            // Register the KeyService as a singleton for MainPage
            builder.Services.AddSingleton<KeyService>();

            // Register pages for dependency injection
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MediaPage>();
            // Register our new example page
            builder.Services.AddTransient<WebPage>();

            return builder.Build();
        }
    }
}
