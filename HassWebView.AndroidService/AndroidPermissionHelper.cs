using Android.Content;
using Android.Provider;
using AndroidX.Core.App;
using Application = Android.App.Application;

namespace HassWebView.AndroidService;

/// <summary>
/// Android-specific helpers for checking and navigating permission settings.
/// </summary>
public static class AndroidPermissionHelper
{
    // ─────────────────────────────────────────────
    // Permission checks
    // ─────────────────────────────────────────────

    /// <summary>
    /// Returns true when the app's notification channel is enabled by the user.
    /// Uses NotificationManagerCompat so it works on API 19+.
    /// </summary>
    public static bool AreNotificationsEnabled()
    {
        var ctx = Application.Context;
        if (ctx == null) return false;
        var nm = NotificationManagerCompat.From(ctx);
        return nm?.AreNotificationsEnabled() ?? false;
    }

    // ─────────────────────────────────────────────
    // System settings navigation
    // ─────────────────────────────────────────────

    /// <summary>
    /// Opens the system Application Details settings for this app.
    /// Covers Location, SMS, Camera, Microphone, and other runtime permissions.
    /// </summary>
    public static void OpenAppDetailsSettings()
    {
        try
        {
            var ctx = Application.Context;
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            intent.SetData(Android.Net.Uri.Parse($"package:{ctx.PackageName}"));
            intent.AddFlags(ActivityFlags.NewTask);
            ctx.StartActivity(intent);
        }
        catch { /* swallow on restricted environments */ }
    }

    /// <summary>
    /// Opens the system Notification Listener (notification access) settings screen.
    /// Required for apps that use NotificationListenerService.
    /// </summary>
    public static void OpenNotificationListenerSettings()
    {
        try
        {
            var ctx = Application.Context;
            var intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
            intent.AddFlags(ActivityFlags.NewTask);
            ctx.StartActivity(intent);
        }
        catch
        {
            // Fallback to app details if the listener settings screen is unavailable
            OpenAppDetailsSettings();
        }
    }

    /// <summary>
    /// Convenience overload: opens the appropriate settings screen based on a permission kind string.
    /// Recognised values: "Notification" → notification listener settings; anything else → app details.
    /// </summary>
    public static void OpenSystemPermission(string kind)
    {
        if (kind == "Notification")
            OpenNotificationListenerSettings();
        else
            OpenAppDetailsSettings();
    }
}
