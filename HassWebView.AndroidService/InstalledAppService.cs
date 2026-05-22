using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using HassWebView.AndroidService.Models;

namespace HassWebView.AndroidService;

/// <summary>
/// Queries the Android PackageManager to enumerate launcher-visible installed apps.
/// </summary>
public static class InstalledAppService
{
    /// <summary>
    /// Returns all apps that appear in the device launcher (ACTION_MAIN + CATEGORY_LAUNCHER).
    /// Icons are loaded synchronously; call this inside Task.Run to avoid blocking the UI thread.
    /// </summary>
    public static List<InstalledAppInfo> GetInstalledApps()
    {
        var result = new List<InstalledAppInfo>();

        var pm = Android.App.Application.Context.PackageManager;
        if (pm is null) return result;

        var intent = new Intent(Intent.ActionMain);
        intent.AddCategory(Intent.CategoryLauncher);

        var activities = pm.QueryIntentActivities(intent, 0);

        foreach (var ri in activities)
        {
            if (ri?.ActivityInfo is null) continue;

            var appInfo = ri.ActivityInfo.ApplicationInfo;
            if (appInfo is null) continue;

            string name        = pm.GetApplicationLabel(appInfo) ?? appInfo.PackageName ?? string.Empty;
            string packageName = appInfo.PackageName ?? string.Empty;

            ImageSource? icon = null;
            try
            {
                var drawable = pm.GetApplicationIcon(appInfo);
                if (drawable is BitmapDrawable bd && bd.Bitmap is not null)
                    icon = ImageSource.FromStream(() =>
                    {
                        var stream = new System.IO.MemoryStream();
                        bd.Bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 90, stream);
                        stream.Seek(0, System.IO.SeekOrigin.Begin);
                        return stream;
                    });
            }
            catch { /* ignore individual icon load failures */ }

            result.Add(new InstalledAppInfo
            {
                Name        = name,
                PackageName = packageName,
                Icon        = icon,
            });
        }

        return result;
    }
}
