using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.AdSkipping
{
    public static class AdSkippingExtensions
    {
        /// <summary>
        /// Enables the GKD-based ad skipping service.
        /// </summary>
        /// <param name="builder">The MauiAppBuilder.</param>
        /// <returns>The MauiAppBuilder for chaining.</returns>
        public static MauiAppBuilder UseAdSkipping(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IAdRulesManager, AdRulesManager>();
            builder.Services.AddSingleton<IMauiInitializeService, AdSkippingInitializeService>();
            return builder;
        }
    }
}
