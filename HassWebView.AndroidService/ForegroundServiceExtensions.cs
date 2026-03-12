using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService
{
    /// <summary>
    /// Provides an extension method to register the foreground service.
    /// </summary>
    public static class ForegroundServiceExtensions
    {
        /// <summary>
        /// Registers the INotificationService with its Android implementation as a singleton.
        /// </summary>
        /// <param name="builder">The MAUI app builder.</param>
        /// <returns>The MAUI app builder for chaining.</returns>
        public static MauiAppBuilder UseForegroundService(this MauiAppBuilder builder)
        {
#if ANDROID
            builder.Services.AddSingleton<INotificationService, NotificationService>();
#endif
            return builder;
        }
    }
}
