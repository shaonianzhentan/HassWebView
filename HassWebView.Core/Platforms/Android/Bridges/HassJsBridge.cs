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
            // VideoService.HtmlWebView(wv, url);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Shell.Current.GoToAsync($"MediaPage?url={System.Web.HttpUtility.UrlEncode(url)}");
            });
        }
    }
}
