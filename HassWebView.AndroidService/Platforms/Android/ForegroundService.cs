using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Collections.Generic;

namespace HassWebView.AndroidService.Platforms.Android
{
    [Service]
    public class ForegroundService : Service
    {
        private const string ChannelId = "ForegroundServiceChannel";
        private const int NotificationId = 1;
        private int _currentIconResource;
        private List<NotificationAction> _currentActions = new List<NotificationAction>();

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");

            CreateNotificationChannel();

            // For both start and update, check if actions are provided.
            bool updateActions = intent.GetBooleanExtra("update_actions", false);
            if (intent.GetStringArrayExtra(NotificationService.ExtraActionId) != null || updateActions)
            {
                var actionIds = intent.GetStringArrayExtra(NotificationService.ExtraActionId) ?? new string[0];
                var actionTitles = intent.GetStringArrayExtra("ACTION_TITLES") ?? new string[0];
                _currentActions.Clear();
                for (int i = 0; i < actionIds.Length; i++)
                {
                    _currentActions.Add(new NotificationAction(actionIds[i], actionTitles[i]));
                }
            }

            if (intent.Action == NotificationService.ActionUpdate)
            {
                var notification = BuildNotification(title, message);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(NotificationId, notification);
            }
            else
            {
                _currentIconResource = intent.GetIntExtra("iconResource", 0);
                var notification = BuildNotification(title, message);
                StartForeground(NotificationId, notification);
            }

            return StartCommandResult.Sticky;
        }

        private Notification BuildNotification(string title, string message)
        {
            var packageName = global::Android.App.Application.Context.PackageName;
            var launchIntent = global::Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
            var pendingIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(this, ChannelId)
               .SetContentTitle(title)
               .SetContentText(message)
               .SetOngoing(true)
               .SetContentIntent(pendingIntent);

            if (_currentIconResource != 0)
            {
                notificationBuilder.SetSmallIcon(_currentIconResource);
            }
            else
            {
                notificationBuilder.SetSmallIcon(global::Android.App.Application.Context.ApplicationInfo.Icon);
            }

            // Add action buttons
            foreach (var action in _currentActions)
            {
                var actionIntent = new Intent(this, typeof(NotificationActionReceiver));
                actionIntent.SetAction(NotificationService.ActionNotificationClick);
                actionIntent.PutExtra(NotificationService.ExtraActionId, action.Id);
                // Each PendingIntent needs a unique request code to be distinguishable.
                var actionPendingIntent = PendingIntent.GetBroadcast(this, action.Id.GetHashCode(), actionIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                notificationBuilder.AddAction(0, action.Title, actionPendingIntent);
            }

            return notificationBuilder.Build();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            StopForeground(true);
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channel = new NotificationChannel(ChannelId, "Foreground Service", NotificationImportance.Default);
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
