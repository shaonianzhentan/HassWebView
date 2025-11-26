using Microsoft.UI.Xaml.Controls;

namespace Maui.HassWebView;

public partial class HassWebViewHandler
{
    protected override void ConnectHandler(WebView2 platformView)
    {
        base.ConnectHandler(platformView);

        platformView.CoreWebView2Initialized += (s, e) =>
        {
            var core = platformView.CoreWebView2;
            if (core != null)
            {
                core.Settings.IsZoomControlEnabled = VirtualView.EnableZoom;
                core.Settings.AreDefaultContextMenusEnabled = true;
            }
        };
    }
}
