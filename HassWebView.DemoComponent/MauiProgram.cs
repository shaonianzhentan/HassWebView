using HassWebView.Component;
using Microsoft.Extensions.Logging;

namespace HassWebView.DemoComponent;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseHassComponents();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
