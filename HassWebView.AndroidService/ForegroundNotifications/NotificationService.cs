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
            NotificationChannelHelper.EnsureChannel(context);

            var intent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(context, NotificationChannelHelper.ChannelId)
                .SetContentTitle(title)
                .SetContentText(content)
                .SetSmallIcon(context.ApplicationInfo.Icon)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            int requestCode = 0;
            foreach (var action in actions)
            {
                var actionIntent = new Intent(context, typeof(NotificationActionReceiver));
                actionIntent.SetAction(action.ActionId);
                var actionPendingIntent = PendingIntent.GetBroadcast(context, requestCode++, actionIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                builder.AddAction(new NotificationCompat.Action.Builder(0, action.Title, actionPendingIntent).Build());
            }

            NotificationManagerCompat.From(context).Notify(notificationId, builder.Build());
        }

        public void CancelNotification(int notificationId)
        {
            var context = global::Android.App.Application.Context;
            NotificationManagerCompat.From(context).Cancel(notificationId);
        }
    }
}
