using System;

namespace HassWebView.Core.Bridges
{
    // This is the Windows-specific implementation of the ExternalApp partial class.
    // It does not need any special attributes or base classes like the Android version.
    public partial class ExternalApp
    {
        // This partial method provides the Windows-specific implementation.
        public void getExternalAuth(string message)
        {
            _authAction?.Invoke("getExternalAuth", message);
            Console.WriteLine($"HassJsBridge.getExternalAuth called on Windows with message: {message}");
        }

        // This partial method also provides the Windows-specific implementation.
        public void revokeExternalAuth(string message)
        {
            _authAction?.Invoke("revokeExternalAuth", message);
            Console.WriteLine($"HassJsBridge.revokeExternalAuth called on Windows with message: {message}");
        }
    }
}
