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
        /// <summary>Checks if the notification listener permission is granted.</summary>
        bool IsPermissionEnabled();

        /// <summary>Opens the system notification access settings screen.</summary>
        void RequestPermission();

        /// <summary>
        /// Returns all user-installed apps (excludes system apps).
        /// Each item may carry a Base64 icon for display in selection UI.
        /// </summary>
        Task<IEnumerable<InstalledAppInfo>> GetInstalledApps();

        /// <summary>Returns the persisted list of selected package names.</summary>
        Task<IEnumerable<string>> GetSelectedApps();

        /// <summary>Persists the user’s app selection.</summary>
        Task SaveSelectedApps(IEnumerable<string> selectedApps);
    }
}
