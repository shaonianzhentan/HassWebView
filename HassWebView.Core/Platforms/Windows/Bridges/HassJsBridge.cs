using HassWebView.Core.Services;
using System;

namespace HassWebView.Core.Bridges
{
    // This is the Windows-specific implementation of the HassJsBridge partial class.
    public partial class HassJsBridge
    {

        public void OpenVideoPlayer(string url)
        {
            VideoService.HtmlWebView(wv, url);
        }
    }
}
