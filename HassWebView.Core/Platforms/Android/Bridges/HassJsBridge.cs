using Android.Webkit;
using HassWebView.Core.Services;
using Java.Interop;

namespace HassWebView.Core.Bridges
{
    // This is the Android-specific implementation of the HassJsBridge partial class.
    public partial class HassJsBridge : Java.Lang.Object
    {

        [JavascriptInterface]
        [Export("OpenVideoPlayer")]
        public void OpenVideoPlayer(string url)
        {
            _action?.Invoke("OpenVideoPlayer", url);
        }
    }
}
