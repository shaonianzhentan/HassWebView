using HassWebView.AndroidService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.NotificationForwarding
{
    /// <summary>
    /// Provides an interface to manage the notification forwarding feature.
    /// </summary>
    public interface INotificationForwardingManager
    {
        /// <summary>
        /// Checks if the notification listener permission is granted for the service.
        /// </summary>
        /// <returns>True if permission is granted, otherwise false.</returns>
        bool IsPermissionEnabled();

        /// <summary>
        /// Opens the system's security settings screen to let the user grant notification access.
        /// </summary>
        void RequestPermission();

        /// <summary>
        /// Gets a list of all user-installed applications that can be launched.
        /// </summary>
        /// <returns>A list of ForwardingAppInfo objects.</returns>
        Task<IEnumerable<ForwardingAppInfo>> GetInstalledApps();

        /// <summary>
        /// Retrieves the list of package names for the apps selected by the user for notification forwarding.
        /// </summary>
        /// <returns>A list of package names.</returns>
        Task<IEnumerable<string>> GetSelectedApps();

        /// <summary>
        /// Saves the user's selection of apps for notification forwarding.
        /// </summary>
        /// <param name="selectedApps">A list of package names to save.</param>
        Task SaveSelectedApps(IEnumerable<string> selectedApps);
    }
}
