using System;

namespace HassWebView.Core.Configuration
{
    public class HassWebViewOptions
    {
        public Action ShowSettingsScreen { get; set; }
        public Action<string> PlayVideo { get; set; }
        public Action<string> OpenMediaPlayer { get; set; }
    }
}
