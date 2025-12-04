using System.Diagnostics;

#if ANDROID
using Android.Content;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.ApplicationModel;
#endif

#if WINDOWS
using System;
using System.Runtime.InteropServices;
#endif

namespace HassWebView.Core
{
#if ANDROID
    public class HassJsBridge : Java.Lang.Object
#elif WINDOWS
    [ComVisible(true)]
    public class HassJsBridge
#else
    public class HassJsBridge
#endif
    {
        public const string BridgeName = "HassJsBridge";

#if ANDROID
        [JavascriptInterface]
        [Export("OpenVideoPlayer")]
#endif
        public void OpenVideoPlayer(string url)
        {
            Debug.WriteLine($"[{GetPlatform()}] OpenVideoPlayer with url: {url}");
            if (string.IsNullOrEmpty(url))
                return;

#if ANDROID
            try
            {
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(global::Android.Net.Uri.Parse(url), "video/*");
                intent.AddFlags(ActivityFlags.NewTask);
                Platform.CurrentActivity?.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting video player: {ex}");
            }
#elif WINDOWS
            try
            {
                // Fire and forget
                _ = global::Windows.System.Launcher.LaunchUriAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting video player: {ex}");
            }
#endif
        }

        private string GetPlatform()
        {
#if ANDROID
            return "Android";
#elif WINDOWS
            return "Windows";
#else
            return "Generic";
#endif
        }
    }
}
