using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using HassWebView.AndroidService.Platforms.Android;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public NotificationService()
        {
            _instance = this;
        }

        public async Task<bool> EnsurePermissionIsGrantedAsync(string permissionDeniedMessage = null)
        {
            var status = await CheckPermissionAsync();
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            status = await RequestPermissionAsync();
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            // If we are here, the permission is denied. Guide the user to settings.
            var message = string.IsNullOrEmpty(permissionDeniedMessage) 
                ? "Notification permission required. Please enable it in settings." 
                : permissionDeniedMessage;

            Toast.MakeText(global::Android.App.Application.Context, message, ToastLength.Long).Show();
            OpenNotificationSettings();

            return false; // Permission is not granted at this point.
        }

        public async Task<bool> StartAsync(string title, string message, int iconResource = 0, IEnumerable<NotificationAction> actions = null)
        {
            var permissionStatus = await CheckPermissionAsync();
            if (permissionStatus != PermissionStatus.Granted)
            {
                return false;
            }

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
