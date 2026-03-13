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
        // Channel IDs
        public const string ForegroundChannelId = "foreground_service_channel";
        public const string NotificationChannelId = "normal_notification_channel";

        // Channel Names
        public const string ForegroundChannelName = "Foreground Service";
        public const string NotificationChannelName = "Notifications";

        // Notification ID
        private const int ForegroundNotificationId = 101;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannels();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent.Action == NotificationService.ActionUpdate)
            {
                UpdateNotification(intent);
            }
            else
            {
                StartForegroundService(intent);
            }

            return StartCommandResult.Sticky;
        }

        private void StartForegroundService(Intent intent)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");

            var notification = BuildNotification(title, message, intent, ForegroundChannelId);

            StartForeground(ForegroundNotificationId, notification);
        }

        private void UpdateNotification(Intent intent)
        {
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("message");

            var notification = BuildNotification(title, message, intent, ForegroundChannelId);

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(ForegroundNotificationId, notification);
        }

        private Notification BuildNotification(string title, string message, Intent intent, string channelId)
        {
            var pendingIntent = BuildPendingIntent();

            var builder = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(ApplicationInfo.Icon)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true);

            var iconResource = intent.GetIntExtra("iconResource", 0);
            if (iconResource != 0)
            {
                builder.SetSmallIcon(iconResource);
            }

            // Always clear existing actions before adding new ones
            builder.MActions.Clear();

            var actionIds = intent.GetStringArrayExtra(NotificationService.ExtraActionId);
            var actionTitles = intent.GetStringArrayExtra(NotificationService.ExtraActionTitles);

            if (actionIds != null && actionTitles != null)
            {
                for (int i = 0; i < actionIds.Length; i++)
                {
                    var actionIntent = new Intent(this, typeof(ActionReceiver));
                    actionIntent.PutExtra(NotificationService.ExtraActionId, actionIds[i]);
                    var pendingActionIntent = PendingIntent.GetBroadcast(this, i, actionIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                    builder.AddAction(0, actionTitles[i], pendingActionIntent);
                }
            }

            return builder.Build();
        }

        private PendingIntent BuildPendingIntent()
        {
            var launchIntent = PackageManager.GetLaunchIntentForPackage(PackageName);
            return PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.Immutable);
        }

        private void CreateNotificationChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            // Foreground Service Channel (Silent)
            var foregroundChannel = new NotificationChannel(ForegroundChannelId, ForegroundChannelName, NotificationImportance.Low)
            {
                Description = "A silent channel for the persistent foreground service notification.",
            };
            notificationManager.CreateNotificationChannel(foregroundChannel);

            // Normal Notifications Channel (With Sound/Vibration)
            var notificationChannel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.Default)
            {
                Description = "Channel for general app notifications."
            };
            notificationManager.CreateNotificationChannel(notificationChannel);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
