using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.AdSkipping
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
