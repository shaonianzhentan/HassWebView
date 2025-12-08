using System;

namespace HassWebView.Core.Bridges
{
    // This is the Windows-specific implementation of the HassJsBridge partial class.
    public partial class HassJsBridge
    {

        public void OpenVideoPlayer(string url, string headers)
        {
            // Similar to Android, the JsBridgeHandler on Windows intercepts this call.
            // This body is not strictly necessary but is good for debugging.
            Console.WriteLine($"HassJsBridge.OpenVideoPlayer call intercepted on Windows.");
        }
    }
}
