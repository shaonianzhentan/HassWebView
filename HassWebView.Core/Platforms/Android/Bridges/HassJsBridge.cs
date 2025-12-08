using Android.Webkit;

namespace HassWebView.Core.Bridges
{
    // This is the Android-specific implementation of the HassJsBridge partial class.
    public partial class HassJsBridge : Java.Lang.Object
    {

        [JavascriptInterface]
        public void OpenVideoPlayer(string url, string headers)
        {
            // The actual implementation for OpenVideoPlayer on Android will be handled by
            // the JsBridgeHandler intercepting the call and invoking the corresponding
            // method on the HassWebView. This body can remain empty or contain logging.
            Console.WriteLine($"HassJsBridge.OpenVideoPlayer call intercepted on Android.");
        }
    }
}
