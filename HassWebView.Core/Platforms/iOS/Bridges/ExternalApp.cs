namespace HassWebView.Core.Bridges
{
    // This is the iOS-specific implementation of the ExternalApp partial class.
    // iOS uses WKWebView which doesn't require special attributes like Android.
    public partial class ExternalApp
    {
        public void getExternalAuth(string message)
        {
            _authAction?.Invoke("getExternalAuth", message);
            Console.WriteLine($"HassJsBridge.getExternalAuth called on iOS with message: {message}");
        }

        public void revokeExternalAuth(string message)
        {
            _authAction?.Invoke("revokeExternalAuth", message);
            Console.WriteLine($"HassJsBridge.revokeExternalAuth called on iOS with message: {message}");
        }

        public void externalBus(string message)
        {
            _authAction?.Invoke("externalBus", message);
        }
    }
}