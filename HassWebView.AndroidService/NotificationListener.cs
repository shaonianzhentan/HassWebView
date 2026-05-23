using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Service.Notification;
using HassWebView.AndroidService.NotificationForwarding;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.AndroidService;

[Service(Name = "HassWebView.AndroidService.NotificationListener",
         Label = "HassWebView Notification Listener",
         Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
[IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
public class NotificationListener : NotificationListenerService
{
    private INotificationForwardingService? _forwardingService;

    public override void OnCreate()
    {
        base.OnCreate();
        // Use GetService (not GetRequiredService) so the listener degrades gracefully
        // when the host app has not registered INotificationForwardingService.
        _forwardingService = MauiApplication.Current.Services.GetService<INotificationForwardingService>();
    }

    public override void OnNotificationPosted(StatusBarNotification sbn)
    {
        if (sbn?.Notification == null || _forwardingService == null) return;

        var extras = sbn.Notification.Extras;
        var title  = extras.GetString(Notification.ExtraTitle);
        var text   = extras.GetString(Notification.ExtraText);

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(text)) return;

        var notificationData = new NotificationData
        {
            PackageName = sbn.PackageName ?? string.Empty,
            PostTime    = sbn.PostTime,
            Title       = title ?? string.Empty,
            Text        = text  ?? string.Empty,
            LargeIcon   = GetBitmapBytes(extras.GetParcelable(Notification.ExtraLargeIcon) as Bitmap),
            Picture     = GetBitmapBytes(extras.GetParcelable(Notification.ExtraPicture) as Bitmap),
        };

        Task.Run(() => _forwardingService.ForwardNotificationAsync(notificationData));
    }

    public override void OnNotificationRemoved(StatusBarNotification sbn)
        => base.OnNotificationRemoved(sbn);

    private static byte[]? GetBitmapBytes(Bitmap? bitmap)
    {
        if (bitmap == null) return null;
        using var stream = new System.IO.MemoryStream();
        bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
        return stream.ToArray();
    }
}
