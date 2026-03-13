using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using AndroidX.Core.App;
using HassWebView.AndroidService.Platforms.Android;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;

namespace HassWebView.AndroidService
{
    public class NotificationService : INotificationService
    {
        public const string ActionUpdate = "UPDATE";
        internal const string ActionNotificationClick = "NOTIFICATION_ACTION_CLICK";
        internal const string ExtraActionId = "ACTION_ID";
        private const string ExtraActionTitles = "ACTION_TITLES";

        public event Action<string> ActionTriggered;

        private static NotificationService _instance;
        private static int _notificationCounter = 0;

        public NotificationService()
        {
            _instance = this;
        }

        public async Task<bool> EnsurePermissionIsGrantedAsync(string permissionDeniedMessage = null)
        {
            var status = await CheckPermissionAsync();
            if (status == PermissionStatus.Granted) return true;

            status = await RequestPermissionAsync();
            if (status == PermissionStatus.Granted) return true;

            var message = string.IsNullOrEmpty(permissionDeniedMessage)
                ? "Notification permission required. Please enable it in settings."
                : permissionDeniedMessage;

            Toast.MakeText(global::Android.App.Application.Context, message, ToastLength.Long).Show();
            OpenNotificationSettings();

            return false;
        }

        public async Task<bool> StartAsync(string title, string message, int iconResource = 0, IEnumerable<NotificationAction> actions = null)
        {
            var permissionStatus = await CheckPermissionAsync();
            if (permissionStatus != PermissionStatus.Granted) return false;

            var intent = new Intent(global::Android.App.Application.Context, typeof(ForegroundService));
            intent.PutExtra("title", title);
            intent.PutExtra("message", message);
            intent.PutExtra("iconResource", iconResource);
            if (actions != null)
            {
                intent.PutStringArrayExtra(ExtraActionId, actions.Select(a => a.Id).ToArray());
                intent.PutStringArrayExtra(ExtraActionTitles, actions.Select(a => a.Title).ToArray());
            }

            global::Android.App.Application.Context.StartService(intent);
            return true;
        }

        #region ShowNotificationAsync Implementation

        public Task<int> ShowNotificationAsync(string title, string message, IEnumerable<NotificationAction> actions = null)
        {
            var id = Interlocked.Increment(ref _notificationCounter);
            return ShowNotificationAsync(id, title, message, null, actions);
        }

        public Task<int> ShowNotificationAsync(string title, string message, string clickUrl, IEnumerable<NotificationAction> actions = null)
        {
            var id = Interlocked.Increment(ref _notificationCounter);
            return ShowNotificationAsync(id, title, message, clickUrl, actions);
        }

        public Task<int> ShowNotificationAsync(int id, string title, string message, IEnumerable<NotificationAction> actions = null)
        {
            return ShowNotificationInternalAsync(id, title, message, null, actions);
        }

        public Task<int> ShowNotificationAsync(int id, string title, string message, string clickUrl, IEnumerable<NotificationAction> actions = null)
        {
            return ShowNotificationInternalAsync(id, title, message, clickUrl, actions);
        }

        private Task<int> ShowNotificationInternalAsync(int id, string title, string message, string clickUrl, IEnumerable<NotificationAction> actions)
        {
            var context = global::Android.App.Application.Context;
            var notificationManager = NotificationManager.FromContext(context);
            var notification = BuildStandardNotification(context, id, title, message, clickUrl, actions);
            notificationManager.Notify(id, notification);
            return Task.FromResult(id);
        }

        #endregion

        private Notification BuildStandardNotification(Context context, int id, string title, string message, string clickUrl, IEnumerable<NotificationAction> actions)
        {
            PendingIntent pendingIntent;

            if (!string.IsNullOrEmpty(clickUrl) && Uri.Parse(clickUrl) != null)
            {
                var urlIntent = new Intent(Intent.ActionView, Uri.Parse(clickUrl));
                pendingIntent = PendingIntent.GetActivity(context, id, urlIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            }
            else
            {
                var launchIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
                pendingIntent = PendingIntent.GetActivity(context, id, launchIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            }

            var builder = new NotificationCompat.Builder(context, ForegroundService.NotificationChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(context.ApplicationInfo.Icon)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);

            if (actions != null)
            {
                foreach (var (action, index) in actions.Select((value, i) => (value, i)))
                {
                    var actionIntent = new Intent(context, typeof(ActionReceiver));
                    actionIntent.PutExtra(ExtraActionId, action.Id);
                    var pendingActionIntent = PendingIntent.GetBroadcast(context, id * 1000 + index, actionIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                    builder.AddAction(0, action.Title, pendingActionIntent);
                }
            }

            return builder.Build();
        }

        public Task UpdateAsync(string title, string message, IEnumerable<NotificationAction> actions = null)
        {
            var intent = new Intent(global::Android.App.Application.Context, typeof(ForegroundService));
            intent.SetAction(ActionUpdate);
            intent.PutExtra("title", title);
            intent.PutExtra("message", message);

            if (actions != null)
            {
                intent.PutStringArrayExtra(ExtraActionId, actions.Select(a => a.Id).ToArray());
                intent.PutStringArrayExtra(ExtraActionTitles, actions.Select(a => a.Title).ToArray());
                intent.PutExtra("update_actions", true);
            }

            global::Android.App.Application.Context.StartService(intent);
            return Task.CompletedTask;
        }

        public void Stop()
        {
            var intent = new Intent(global::Android.App.Application.Context, typeof(ForegroundService));
            global::Android.App.Application.Context.StopService(intent);
        }

        public Task<PermissionStatus> CheckPermissionAsync()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                return Permissions.CheckAsync<Permissions.Notifications>();
            }
            return Task.FromResult(PermissionStatus.Granted);
        }

        public Task<PermissionStatus> RequestPermissionAsync()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                return Permissions.RequestAsync<Permissions.Notifications>();
            }
            return Task.FromResult(PermissionStatus.Granted);
        }

        public void OpenNotificationSettings()
        {
            var intent = new Intent(Settings.ActionAppNotificationSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            intent.PutExtra(Settings.ExtraAppPackage, global::Android.App.Application.Context.PackageName);
            global::Android.App.Application.Context.StartActivity(intent);
        }

        internal static void TriggerAction(string actionId)
        {
            _instance?.ActionTriggered?.Invoke(actionId);
        }
    }
}
