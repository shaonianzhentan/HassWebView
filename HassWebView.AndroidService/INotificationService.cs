using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HassWebView.AndroidService
{
    /// <summary>
    /// Defines the contract for a foreground notification service.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Triggered when a notification action button is clicked.
        /// The string argument is the ID of the action that was triggered.
        /// </summary>
        event Action<string> ActionTriggered;

        /// <summary>
        /// Ensures that the required notification permission is granted. 
        /// It handles checking, requesting, and guiding the user to the app settings if the permission is denied.
        /// </summary>
        /// <param name="permissionDeniedMessage">Optional: The message to show in a Toast when the user is being guided to the settings. A default message will be used if not provided.</param>
        /// <returns>True if permission is granted, false otherwise.</returns>
        Task<bool> EnsurePermissionIsGrantedAsync(string permissionDeniedMessage = null);

        /// <summary>
        /// Starts the foreground service and displays a notification.
        /// It will check for notification permission before starting.
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The message content of the notification.</param>
        /// <param name="iconResource">Optional: The resource ID of the icon to use for the notification. If not provided, the application icon will be used.</param>
        /// <param name="actions">Optional: A list of actions to display as buttons on the notification.</param>
        /// <returns>True if the service was started successfully, false otherwise (e.g., permission denied).</returns>
        Task<bool> StartAsync(string title, string message, int iconResource = 0, IEnumerable<NotificationAction> actions = null);

        /// <summary>
        /// Updates the content of the existing notification.
        /// </summary>
        /// <param name="title">The new title for the notification.</param>
        /// <param name="message">The new message for the notification.</param>
        /// <param name="actions">Optional: A new list of actions. If null, existing actions are preserved.</param>
        Task UpdateAsync(string title, string message, IEnumerable<NotificationAction> actions = null);

        /// <summary>
        /// Stops the foreground service.
        /// </summary>
        void Stop();

        /// <summary>
        /// Checks the current status of the notification permission.
        /// </summary>
        /// <returns>The current permission status.</returns>
        Task<Microsoft.Maui.ApplicationModel.PermissionStatus> CheckPermissionAsync();

        /// <summary>
        /// Requests the notification permission from the user.
        /// </summary>
        /// <returns>The permission status after the request.</returns>
        Task<Microsoft.Maui.ApplicationModel.PermissionStatus> RequestPermissionAsync();

        /// <summary>
        /// Opens the application's settings screen for the user.
        /// This is useful for prompting the user to grant a permission that they have permanently denied.
        /// </summary>
        void OpenNotificationSettings();
    }
}
