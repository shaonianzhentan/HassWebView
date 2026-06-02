using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Service.Notification;
using HassWebView.AndroidService.NotificationForwarding;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.AndroidService;

[Service(Label = "HassWebView Notification Listener",
         Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE", 
         Exported = true)]
[IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
public class NotificationListener : NotificationListenerService
{
    private INotificationForwardingService? _forwardingService;

    public override void OnCreate()
    {
        base.OnCreate();
        // Use GetService (not GetRequiredService) so the listener degrades gracefully
        // when the host app has not registered INotificationForwardingService.
#pragma warning disable CS0618 // Type or member is obsolete
        _forwardingService = MauiApplication.Current.Services.GetService<INotificationForwardingService>();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override void OnNotificationPosted(StatusBarNotification? sbn)
    {
        if (sbn?.Notification == null || _forwardingService == null) return;

        var extras = sbn.Notification.Extras;
        if (extras == null) return;
        
        var title  = extras.GetString(Notification.ExtraTitle);
        var text   = extras.GetString(Notification.ExtraText);

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(text)) return;

        var notificationData = new NotificationData
        {
            PackageName = sbn.PackageName ?? string.Empty,
            PostTime    = sbn.PostTime,
            Title       = title ?? string.Empty,
            Text        = text  ?? string.Empty,
            LargeIcon   = GetBitmapBytes(GetParcelableBitmap(extras, GetExtraLargeIconKey())),
            Picture     = GetBitmapBytes(GetParcelableBitmap(extras, Notification.ExtraPicture)),
        };

        Task.Run(() => _forwardingService.ForwardNotificationAsync(notificationData));
    }

    public override void OnNotificationRemoved(StatusBarNotification? sbn)
        => base.OnNotificationRemoved(sbn);

    private static string GetExtraLargeIconKey()
    {
#pragma warning disable CA1422 // Validate platform compatibility
        return Notification.ExtraLargeIcon;
#pragma warning restore CA1422 // Validate platform compatibility
    }

    private static Bitmap? GetParcelableBitmap(Bundle extras, string key)
    {
#pragma warning disable CA1416, CA1422 // Validate platform compatibility
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var obj = extras.GetParcelable(key, Java.Lang.Class.FromType(typeof(Bitmap)));
            return obj as Bitmap;
        }
        else
        {
            return extras.GetParcelable(key) as Bitmap;
        }
#pragma warning restore CA1416, CA1422 // Validate platform compatibility
    }

    private static byte[]? GetBitmapBytes(Bitmap? bitmap)
    {
        if (bitmap == null) return null;
        using var stream = new System.IO.MemoryStream();
        bitmap.Compress(Bitmap.CompressFormat.Png!, 100, stream);
        return stream.ToArray();
    }
}
