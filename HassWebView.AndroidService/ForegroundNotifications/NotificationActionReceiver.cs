using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationActionReceiver : BroadcastReceiver
    {
        public const string ActionIdKey = "NotificationActionId";
        public const string NotificationIdKey = "NotificationId";

        public override void OnReceive(Context context, Intent intent)
        {
            var actionId = intent.Action;
            if (string.IsNullOrEmpty(actionId))
            {
                return;
            }

            // Dismiss the notification that triggered the action
            var notificationId = intent.GetIntExtra(NotificationIdKey, -1);
            if (notificationId != -1)
            {
                var notificationManager = NotificationManagerCompat.From(context);
                notificationManager.Cancel(notificationId);
            }

            // Close the notification shade
            context.SendBroadcast(new Intent(Intent.ActionCloseSystemDialogs));

            // Launch the main activity
            var launchIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            if (launchIntent != null)
            {
                launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                launchIntent.PutExtra(ActionIdKey, actionId); // Pass the action ID to the activity
                context.StartActivity(launchIntent);
            }
        }
    }
}
