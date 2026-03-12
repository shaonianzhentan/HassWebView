using Android.App;
using Android.Content;

namespace HassWebView.AndroidService.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class NotificationActionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Action == NotificationService.ActionNotificationClick)
            {
                var actionId = intent.GetStringExtra(NotificationService.ExtraActionId);
                if (!string.IsNullOrEmpty(actionId))
                {                    
                    NotificationService.TriggerAction(actionId);
                }
            }
        }
    }
}
