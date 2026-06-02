using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationActionReceiver : BroadcastReceiver
    {
        public const string ActionIdKey = "NotificationActionId";
        public const string NotificationIdKey = "NotificationId";

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (context == null || intent == null) return;

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
                notificationManager?.Cancel(notificationId);
            }

            // Close the notification shade (deprecated in Android 31+)
#pragma warning disable CA1422 // Validate platform compatibility
            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
            {
                context.SendBroadcast(new Intent(Intent.ActionCloseSystemDialogs));
            }
#pragma warning restore CA1422 // Validate platform compatibility

            // Launch the main activity
            var packageName = context.PackageName ?? string.Empty;
            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(packageName);
            if (launchIntent != null)
            {
                launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                launchIntent.PutExtra(ActionIdKey, actionId);
                context.StartActivity(launchIntent);
            }
        }
    }
}
