using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    [Service]
    public class ForegroundService : Service
    {
        public const string ActionStart = "START";
        public const string ActionStop  = "STOP";
        public const string ExtraTitle  = "TITLE";
        public const string ExtraText   = "TEXT";

        private const int NotificationId = 1;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ActionStart)
            {
                var title = intent.GetStringExtra(ExtraTitle) ?? "HassWebView is running";
                var text  = intent.GetStringExtra(ExtraText)  ?? "The foreground service is active.";

                NotificationChannelHelper.EnsureChannel(this);

                var notification = new NotificationCompat.Builder(this, NotificationChannelHelper.ChannelId)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetSmallIcon(Application.Context.ApplicationInfo.Icon)
                    .Build();

                StartForeground(NotificationId, notification);
            }
            else if (intent?.Action == ActionStop)
            {
                StopForeground(true);
                StopSelfResult(startId);
            }

            return StartCommandResult.Sticky;
        }
    }
}
