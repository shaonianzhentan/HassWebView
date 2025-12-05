using System.Diagnostics;
using Android.Content;
using Android.Webkit;
using Java.Interop;

namespace HassWebView.Core.Platforms.Android
{
    public class HassJsBridge : Java.Lang.Object
    {
        public const string BridgeName = "HassJsBridge";

        [JavascriptInterface]
        [Export("OpenVideoPlayer")]
        public void OpenVideoPlayer(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(global::Android.Net.Uri.Parse(url), "video/*");
                Platform.CurrentActivity?.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting video player: {ex}");
            }
        }
    }
}
