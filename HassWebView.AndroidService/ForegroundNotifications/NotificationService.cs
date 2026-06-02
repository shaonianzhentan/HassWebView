using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using HassWebView.AndroidService.Notifications;
using System.Collections.Generic;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string content, int notificationId, List<NotificationAction> actions)
        {
            var context = global::Android.App.Application.Context;
            if (context == null) return;

            NotificationChannelHelper.EnsureChannel(context);

            var intent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName ?? string.Empty);
#pragma warning disable CA1416 // Validate platform compatibility
            var flags = Build.VERSION.SdkInt >= BuildVersionCodes.M 
                ? PendingIntentFlags.Immutable 
                : PendingIntentFlags.UpdateCurrent;
#pragma warning restore CA1416 // Validate platform compatibility
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, flags);

            #pragma warning disable CS8602 // Dereference of a possibly null reference
            var builder = new NotificationCompat.Builder(context, NotificationChannelHelper.ChannelId!)
                .SetContentTitle(title)
                .SetContentText(content)
                .SetSmallIcon(context.ApplicationInfo?.Icon ?? global::Android.Resource.Drawable.SymDefAppIcon)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            int requestCode = 0;
            foreach (var action in actions)
            {
                var actionIntent = new Intent(context, typeof(NotificationActionReceiver));
                actionIntent.SetAction(action.ActionId);
#pragma warning disable CA1416 // Validate platform compatibility
                var actionFlags = Build.VERSION.SdkInt >= BuildVersionCodes.M 
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable 
                    : PendingIntentFlags.UpdateCurrent;
#pragma warning restore CA1416 // Validate platform compatibility
                var actionPendingIntent = PendingIntent.GetBroadcast(context, requestCode++, actionIntent, actionFlags);
                builder.AddAction(new NotificationCompat.Action.Builder(0, action.Title ?? string.Empty, actionPendingIntent!).Build());
            }

            var notificationManager = NotificationManagerCompat.From(context);
            var notification = builder.Build();
            if (notification != null)
            {
                notificationManager?.Notify(notificationId, notification);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference
        }

        public void CancelNotification(int notificationId)
        {
            var context = global::Android.App.Application.Context;
            if (context == null) return;
            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager?.Cancel(notificationId);
        }
    }
}
