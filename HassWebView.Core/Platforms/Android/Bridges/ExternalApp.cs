using Android.Webkit;
using Java.Interop;

namespace HassWebView.Core.Bridges
{
    // Android �ض�ʵ�ֲ��֣��̳� Java.Lang.Object �Լ��� AddJavascriptInterface
    public partial class ExternalApp : Java.Lang.Object
    {
        // ʵ�ֲַ����������� [JavascriptInterface] ���Թ� JS ����
        [JavascriptInterface]
        [Export("getExternalAuth")]
        public void getExternalAuth(string message)
        {
            _authAction?.Invoke("getExternalAuth", message);
            Console.WriteLine($"HassJsBridge.getExternalAuth called on Android with message: {message}");
        }

        [JavascriptInterface]
        [Export("revokeExternalAuth")]
        public void revokeExternalAuth(string message)
        {
            _authAction?.Invoke("revokeExternalAuth", message);
            Console.WriteLine($"HassJsBridge.revokeExternalAuth called on Android with message: {message}");
        }

        [JavascriptInterface]
        [Export("externalBus")]
        public void externalBus(string message)
        {
            _authAction?.Invoke("externalBus", message);
        }
    }
}