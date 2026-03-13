using Android.App;
using Android.Content;
using Android.Service.Notification;
using HassWebView.AndroidService.NotificationForwarding;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Diagnostics;
using System.Text.Json;

namespace HassWebView.AndroidService.Platforms.Android
{
    [Service(Label = "HassWebView Notification Listener", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE", Exported = true)]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NotificationForwardingService : NotificationListenerService
    {
        private const string SelectedAppsKey = "notification_forwarding_selected_apps";
        private IEnumerable<string> _selectedApps = new List<string>();

        public override void OnCreate()
        {
            base.OnCreate();
            LoadSelectedApps();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            base.OnNotificationPosted(sbn);

            // Ignore self notifications or if the app is not in the selected list
            if (sbn.PackageName == Application.Context.PackageName || !_selectedApps.Contains(sbn.PackageName))
            {
                return;
            }

            var notification = sbn.Notification;
            if (notification == null) return;

            var extras = notification.Extras;
            if (extras == null) return;

            string title = extras.GetString(Notification.ExtraTitle);
            string text = extras.GetString(Notification.ExtraText);
            string appName = sbn.PackageName;

            // TODO: Here you can forward the notification to your desired target,
            // for example, by calling a method in a shared service or using an event bus.

            Debug.WriteLine($"[NotificationListener] From: {appName}");
            Debug.WriteLine($"[NotificationListener] Title: {title}");
            Debug.WriteLine($"[NotificationListener] Text: {text}");
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        { 
            base.OnNotificationRemoved(sbn);
            // Optional: Handle notification removal if needed
        }

        // Reload the list of selected apps when settings change
        // This could be triggered by a broadcast message from your app
        private void LoadSelectedApps()
        {
            var json = Application.Context.GetSharedPreferences(Application.Context.PackageName, FileCreationMode.Private).GetString(SelectedAppsKey, "[]");
            _selectedApps = JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? new List<string>();
        }
    }
}
