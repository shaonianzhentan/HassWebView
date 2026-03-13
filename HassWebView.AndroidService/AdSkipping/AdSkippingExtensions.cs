using Microsoft.Maui.Hosting;
#if ANDROID
using HassWebView.AndroidService.Platforms.Android;
#endif

namespace HassWebView.AndroidService.AdSkipping
{
    public static class AdSkippingExtensions
    {
        /// <summary>
        /// Enables the GKD-based ad skipping service.
        /// </summary>
        /// <param name="builder">The MauiAppBuilder.</param>
        /// <param name="ruleUrl">The URL to the GKD subscription file (e.g., gkd.json5).</param>
        /// <returns>The MauiAppBuilder for chaining.</returns>
        public static MauiAppBuilder UseAdSkipping(this MauiAppBuilder builder, string ruleUrl)
        {
            if (string.IsNullOrWhiteSpace(ruleUrl))
            {
                return builder;
            }

#if ANDROID
            // Asynchronously load the rules from the provided URL.
            // This happens in the background when the app starts.
            AdSkippingService.RuleManager.LoadRulesFromUrl(ruleUrl);
#endif

            return builder;
        }
    }
}
