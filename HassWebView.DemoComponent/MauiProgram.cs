using HassWebView.Component;
using HassWebView.DemoComponent.Views;
using Microsoft.Extensions.Logging;

namespace HassWebView.DemoComponent;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseHassComponents()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        Routing.RegisterRoute(nameof(AlertExamplePage), typeof(AlertExamplePage));
        Routing.RegisterRoute(nameof(ChipExamplePage), typeof(ChipExamplePage));
        Routing.RegisterRoute(nameof(ControlSwitchExamplePage), typeof(ControlSwitchExamplePage));
        Routing.RegisterRoute(nameof(InputExamplePage), typeof(InputExamplePage));
        Routing.RegisterRoute(nameof(SpinnerExamplePage), typeof(SpinnerExamplePage));
        Routing.RegisterRoute(nameof(GaugeExamplePage), typeof(GaugeExamplePage));
        Routing.RegisterRoute(nameof(ButtonCardExamplePage), typeof(ButtonCardExamplePage));
        Routing.RegisterRoute(nameof(ButtonGridExamplePage), typeof(ButtonGridExamplePage));
        Routing.RegisterRoute(nameof(DetailCardExamplePage), typeof(DetailCardExamplePage));
        Routing.RegisterRoute(nameof(DialogExamplePage), typeof(DialogExamplePage));
        Routing.RegisterRoute(nameof(EntityRowExamplePage), typeof(EntityRowExamplePage));
        Routing.RegisterRoute(nameof(EntityListGroupExamplePage), typeof(EntityListGroupExamplePage));
        Routing.RegisterRoute(nameof(SliderCardExamplePage), typeof(SliderCardExamplePage));
        Routing.RegisterRoute(nameof(StateBadgeExamplePage), typeof(StateBadgeExamplePage));
        Routing.RegisterRoute(nameof(ThemeSelectorExamplePage), typeof(ThemeSelectorExamplePage));
        Routing.RegisterRoute(nameof(SizeSelectorExamplePage), typeof(SizeSelectorExamplePage));
        Routing.RegisterRoute(nameof(SettingsGroupExamplePage), typeof(SettingsGroupExamplePage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
