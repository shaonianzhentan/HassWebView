using Android.Webkit;

namespace HassWebView.Core.Bridges
{
    // Android 特定实现部分：继承 Java.Lang.Object 以兼容 AddJavascriptInterface
    public partial class ExternalApp : Java.Lang.Object
    {
        // 实现分部方法，添加 [JavascriptInterface] 特性供 JS 调用
        [JavascriptInterface]
        public void getExternalAuth(string message)
        {
            _authAction?.Invoke("getExternalAuth", message);
            Console.WriteLine($"HassJsBridge.getExternalAuth called on Android with message: {message}");
        }

        [JavascriptInterface]
        public void revokeExternalAuth(string message)
        {
            _authAction?.Invoke("revokeExternalAuth", message);
            Console.WriteLine($"HassJsBridge.revokeExternalAuth called on Android with message: {message}");
        }
    }
}