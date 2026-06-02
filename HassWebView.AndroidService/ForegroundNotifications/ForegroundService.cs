using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Application = Android.App.Application;

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

        public override IBinder? OnBind(Intent? intent) => null;

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ActionStart)
            {
                var title = intent.GetStringExtra(ExtraTitle) ?? "HassWebView is running";
                var text  = intent.GetStringExtra(ExtraText)  ?? "The foreground service is active.";

                NotificationChannelHelper.EnsureChannel(this);

#pragma warning disable CS8602 // Dereference of a possibly null reference
                var channelId = NotificationChannelHelper.ChannelId!;
                var builder = new NotificationCompat.Builder(this, channelId)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetSmallIcon(Application.Context.ApplicationInfo?.Icon ?? global::Android.Resource.Drawable.SymDefAppIcon);

                var notification = builder.Build();
                if (notification != null)
                {
                    StartForeground(NotificationId, notification);
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference
            }
            else if (intent?.Action == ActionStop)
            {
#pragma warning disable CA1422 // Validate platform compatibility
                StopForeground(true);
#pragma warning restore CA1422 // Validate platform compatibility
                StopSelfResult(startId);
            }

            return StartCommandResult.Sticky;
        }
    }
}
