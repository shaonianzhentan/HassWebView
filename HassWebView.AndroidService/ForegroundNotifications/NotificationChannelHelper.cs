using Android.App;
using Android.OS;
using AndroidX.Core.App;

namespace HassWebView.AndroidService.ForegroundNotifications;

/// <summary>
/// Shared helper for the notification channel used by all foreground/persistent notifications.
/// </summary>
internal static class NotificationChannelHelper
{
    internal static readonly string ChannelId   = "HassWebView_Channel";
    internal static readonly string ChannelName = "HassWebView";

    /// <summary>
    /// Creates the notification channel on Android O+. Safe to call multiple times.
    /// </summary>
    internal static void EnsureChannel(Android.Content.Context context)
    {
        if (context == null || Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var nm = NotificationManagerCompat.From(context);
        if (nm?.GetNotificationChannel(ChannelId) is not null) return;
        if (nm == null) return;

#pragma warning disable CA1416 // Validate platform compatibility
        var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Default);
        nm.CreateNotificationChannel(channel);
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
