using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Provider;
using HassWebView.AndroidService.Models;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;

namespace HassWebView.AndroidService.NotificationForwarding
{
    public class NotificationForwardingManager : INotificationForwardingManager
    {
        private const string SelectedAppsKey = "notification_forwarding_selected_apps";

        public bool IsPermissionEnabled()
        {
            var context = Platform.AppContext;
            string? enabledListeners = Settings.Secure.GetString(context.ContentResolver, "enabled_notification_listeners");

            if (string.IsNullOrEmpty(enabledListeners))
            {
                return false;
            }

            return enabledListeners.Contains(context.PackageName);
        }

        public void RequestPermission()
        {
            var intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
            intent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
        }

        public async Task<IEnumerable<AppInfo>> GetInstalledApps()
        {
            return await Task.Run(() =>
            {
                var pm = Platform.AppContext.PackageManager;
                var packages = pm.GetInstalledApplications(PackageInfoFlags.MatchAll);
                var apps = new List<AppInfo>();

                foreach (var packageInfo in packages)
                {
                    // Filter out system apps
                    if ((packageInfo.Flags & ApplicationInfoFlags.System) == 0)
                    {
                        var appName = packageInfo.LoadLabel(pm).ToString();
                        var packageName = packageInfo.PackageName;
                        var icon = packageInfo.LoadIcon(pm);

                        string? iconBase64 = null;
                        if (icon is BitmapDrawable bitmapDrawable)
                        {
                            using var stream = new MemoryStream();
                            bitmapDrawable.Bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
                            iconBase64 = Convert.ToBase64String(stream.ToArray());
                        }

                        apps.Add(new AppInfo
                        {
                            AppName = appName,
                            PackageName = packageName,
                            IconBase64 = iconBase64
                        });
                    }
                }
                return apps.OrderBy(app => app.AppName).ToList();
            });
        }

        public Task<IEnumerable<string>> GetSelectedApps()
        {
            var json = Preferences.Get(SelectedAppsKey, "[]");
            var appList = JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? new List<string>();
            return Task.FromResult(appList);
        }

        public Task SaveSelectedApps(IEnumerable<string> selectedApps)
        {
            var json = JsonSerializer.Serialize(selectedApps);
            Preferences.Set(SelectedAppsKey, json);
            return Task.CompletedTask;
        }
    }
}
