using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.AndroidService.NotificationForwarding
{
    /// <summary>
    /// Provides extension methods for setting up notification forwarding.
    /// </summary>
    public static class NotificationForwardingExtensions
    {
        /// <summary>
        /// Adds and configures the notification forwarding service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="callback">The callback to be invoked when a notification is forwarded. It receives the service provider and notification data.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection UseNotificationForwarding(this IServiceCollection services, Func<IServiceProvider, NotificationData, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Register the service using a factory. This allows us to resolve the IServiceProvider
            // and pass it to the user's callback, enabling access to other DI services.
            services.AddSingleton<INotificationForwardingService>(sp =>
            {
                // Create the final callback delegate that closes over the service provider (sp).
                Func<NotificationData, Task> finalCallback = (notificationData) => callback(sp, notificationData);

                // Instantiate the service with the final callback.
                return new NotificationForwardingService(finalCallback);
            });

            return services;
        }
    }
}
