using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using HassWebView.AndroidService.Notifications;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    [Service]
    public class ForegroundService : Service
    {
        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var notification = new NotificationCompat.Builder(this, "HassWebView_Channel")
                .SetContentTitle("HassWebView is running")
                .SetContentText("The foreground service is active.")
                .SetSmallIcon(Microsoft.Maui.ApplicationModel.Platform.AppContext.ApplicationInfo.Icon) // Replace with your app icon
                .Build();

            StartForeground(1, notification);

            return StartCommandResult.Sticky;
        }
    }
}
