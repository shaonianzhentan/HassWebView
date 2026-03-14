using Microsoft.Maui.Hosting;
// Add a using alias to resolve the ambiguity between System.Action and Android.Views.Accessibility.Action
using Action = System.Action;


namespace HassWebView.AndroidService.Platforms.Android
{
    public static class AdSkippingExtensions
    {
        public static MauiAppBuilder UseAdSkipping(this MauiAppBuilder builder)
        {
            // Register the single manager for the entire feature
            builder.Services.AddSingleton<IAdSkippingManager, AdSkippingManager>();

            // Register the startup service that uses the manager
            builder.Services.AddSingleton<IMauiInitializeService, AdSkippingInitializeService>();

            return builder;
        }
    }
}
