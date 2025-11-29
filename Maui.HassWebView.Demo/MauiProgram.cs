using Maui.HassWebView.Core;
using Microsoft.Extensions.Logging;

namespace Maui.HassWebView.Demo
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
                // Enable the remote control service with an optional down interval customization.
                // All keys will be intercepted by default.
                // You can control whether a key is handled by the system 
                // within the event handlers (e.g., OnSingleClick) in MainPage.xaml.cs
                // by setting e.Handled = false.
                .UseRemoteControl();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
