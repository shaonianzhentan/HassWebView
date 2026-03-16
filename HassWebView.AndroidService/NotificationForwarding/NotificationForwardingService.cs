using System;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.NotificationForwarding
{
    /// <summary>
    /// Implements the service for forwarding notifications to a registered callback.
    /// This is an internal class, configured and exposed via the INotificationForwardingService interface.
    /// </summary>
    internal class NotificationForwardingService : INotificationForwardingService
    {
        private readonly Func<NotificationData, Task> _callback;

        // The callback is injected via the constructor.
        public NotificationForwardingService(Func<NotificationData, Task> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Forwards notification data to the configured callback.
        /// </summary>
        /// <param name="data">The notification data to forward.</param>
        public async Task ForwardNotificationAsync(NotificationData data)
        {            
            await _callback(data);
        }
    }
}
