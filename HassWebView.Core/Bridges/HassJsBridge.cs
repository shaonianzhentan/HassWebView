using System;

namespace HassWebView.Core.Bridges
{
    // This is the shared part of the HassJsBridge class.
    // The class and its platform-specific methods are marked as partial.
    public partial class HassJsBridge
    {
        private readonly Action<string, string> _action;

        // A parameterless constructor is required for a clean, decoupled design.
        public HassJsBridge(Action<string, string> action) {
            _action = action;
        }
    }
}
