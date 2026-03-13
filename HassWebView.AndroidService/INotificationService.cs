using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HassWebView.AndroidService
{
    public interface INotificationService
    {
        event Action<string> ActionTriggered;

        Task<bool> EnsurePermissionIsGrantedAsync(string permissionDeniedMessage = null);

        Task<bool> StartAsync(string title, string message, int iconResource = 0, IEnumerable<NotificationAction> actions = null);

        #region ShowNotificationAsync Overloads

        // --- Auto-ID Generation --- //

        /// <summary>
        /// Displays a notification that opens the app, using an auto-generated ID.
        /// </summary>
        /// <returns>The unique ID generated for the notification.</returns>
        Task<int> ShowNotificationAsync(string title, string message, IEnumerable<NotificationAction> actions = null);

        /// <summary>
        /// Displays a notification that opens a URL, using an auto-generated ID.
        /// </summary>
        /// <returns>The unique ID generated for the notification.</returns>
        Task<int> ShowNotificationAsync(string title, string message, string clickUrl, IEnumerable<NotificationAction> actions = null);

        // --- Manual ID Specification --- //

        /// <summary>
        /// Displays a notification that opens the app, using a specific ID.
        /// </summary>
        /// <returns>The ID used for the notification.</returns>
        Task<int> ShowNotificationAsync(int id, string title, string message, IEnumerable<NotificationAction> actions = null);

        /// <summary>
        /// Displays a notification that opens a URL, using a specific ID.
        /// </summary>
        /// <returns>The ID used for the notification.</returns>
        Task<int> ShowNotificationAsync(int id, string title, string message, string clickUrl, IEnumerable<NotificationAction> actions = null);

        #endregion

        Task UpdateAsync(string title, string message, IEnumerable<NotificationAction> actions = null);

        void Stop();

        Task<Microsoft.Maui.ApplicationModel.PermissionStatus> CheckPermissionAsync();

        Task<Microsoft.Maui.ApplicationModel.PermissionStatus> RequestPermissionAsync();

        void OpenNotificationSettings();
    }
}
