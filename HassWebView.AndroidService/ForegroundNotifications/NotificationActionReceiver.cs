using Android.Content;

namespace HassWebView.AndroidService.Platforms.Android.Notifications
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationActionReceiver : BroadcastReceiver
    {        
        public override void OnReceive(Context context, Intent intent)
        {
            // You can handle different actions based on intent.Action
            var actionId = intent.Action;
            // Implement your logic here, e.g., send a message to your app
            // or perform a background task.
        }
    }
}
