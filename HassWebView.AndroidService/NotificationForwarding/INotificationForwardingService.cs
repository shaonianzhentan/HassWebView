using System.Threading.Tasks;

namespace HassWebView.AndroidService.NotificationForwarding
{
    /// <summary>
    /// Defines a service for forwarding notifications.
    /// </summary>
    public interface INotificationForwardingService
    {
        /// <summary>
        /// Forwards notification data to a configured callback.
        /// </summary>
        /// <param name="data">The notification data to forward.</param>
        Task ForwardNotificationAsync(NotificationData data);
    }
}
