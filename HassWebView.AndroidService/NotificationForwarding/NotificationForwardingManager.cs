using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Provider;
using HassWebView.AndroidService.Models;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;

namespace HassWebView.AndroidService.NotificationForwarding;

public class NotificationForwardingManager : INotificationForwardingManager
{
    private const string SelectedAppsKey = "notification_forwarding_selected_apps";

    // ── Permission ────────────────────────────────

    public bool IsPermissionEnabled()
    {
        var context          = Platform.AppContext;
        var enabledListeners = Settings.Secure.GetString(context.ContentResolver, "enabled_notification_listeners");
        return !string.IsNullOrEmpty(enabledListeners) && enabledListeners.Contains(context.PackageName);
    }

    public void RequestPermission()
    {
        var intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
        intent.AddFlags(ActivityFlags.NewTask);
        Platform.AppContext.StartActivity(intent);
    }

    // ── App enumeration ───────────────────────────

    /// <summary>
    /// Returns all launcher-visible apps with <see cref="ImageSource"/> icons.
    /// Blocking — call inside Task.Run.
    /// </summary>
    public static List<InstalledAppInfo> GetLauncherApps()
    {
        var result = new List<InstalledAppInfo>();
        var pm     = Android.App.Application.Context.PackageManager;
        if (pm is null) return result;

        var intent = new Intent(Intent.ActionMain);
        intent.AddCategory(Intent.CategoryLauncher);

        foreach (var ri in pm.QueryIntentActivities(intent, 0))
        {
            var appInfo = ri?.ActivityInfo?.ApplicationInfo;
            if (appInfo is null) continue;

            string name        = pm.GetApplicationLabel(appInfo) ?? appInfo.PackageName ?? string.Empty;
            string packageName = appInfo.PackageName ?? string.Empty;

            ImageSource? icon = null;
            try
            {
                if (pm.GetApplicationIcon(appInfo) is BitmapDrawable bd && bd.Bitmap is not null)
                    icon = ImageSource.FromStream(() =>
                    {
                        var stream = new System.IO.MemoryStream();
                        bd.Bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 90, stream);
                        stream.Seek(0, System.IO.SeekOrigin.Begin);
                        return stream;
                    });
            }
            catch { /* ignore icon failures */ }

            result.Add(new InstalledAppInfo { Name = name, PackageName = packageName, Icon = icon });
        }

        return result;
    }

    /// <summary>
    /// Returns all user-installed apps (no system apps) with Base64 PNG icons.
    /// Blocking — call inside Task.Run.
    /// </summary>
    public async Task<IEnumerable<InstalledAppInfo>> GetInstalledApps()
        => await Task.Run(GetUserAppsWithBase64Icons);

    private static List<InstalledAppInfo> GetUserAppsWithBase64Icons()
    {
        var result = new List<InstalledAppInfo>();
        var pm     = Android.App.Application.Context.PackageManager;
        if (pm is null) return result;

        foreach (var appInfo in pm.GetInstalledApplications(PackageInfoFlags.MatchAll))
        {
            if ((appInfo.Flags & ApplicationInfoFlags.System) != 0) continue;

            string name        = pm.GetApplicationLabel(appInfo) ?? appInfo.PackageName ?? string.Empty;
            string packageName = appInfo.PackageName ?? string.Empty;

            string? iconBase64 = null;
            try
            {
                if (appInfo.LoadIcon(pm) is BitmapDrawable bd && bd.Bitmap is not null)
                {
                    using var stream = new System.IO.MemoryStream();
                    bd.Bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 90, stream);
                    iconBase64 = Convert.ToBase64String(stream.ToArray());
                }
            }
            catch { /* ignore icon failures */ }

            result.Add(new InstalledAppInfo { Name = name, PackageName = packageName, IconBase64 = iconBase64 });
        }

        return [.. result.OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)];
    }

    // ── Persistence ───────────────────────────────

    public Task<IEnumerable<string>> GetSelectedApps()
    {
        var json    = Preferences.Get(SelectedAppsKey, "[]");
        var appList = JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? [];
        return Task.FromResult(appList);
    }

    public Task SaveSelectedApps(IEnumerable<string> selectedApps)
    {
        Preferences.Set(SelectedAppsKey, JsonSerializer.Serialize(selectedApps));
        return Task.CompletedTask;
    }
}
