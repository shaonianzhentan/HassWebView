using System;
using Microsoft.Maui.Handlers;

namespace Maui.HassWebView.Core.Platforms.Windows;

using WebView = Microsoft.UI.Xaml.Controls.WebView2;


public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    public static PropertyMapper Mapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            var url = (view.Source as UrlWebViewSource)?.Url;
            if (!string.IsNullOrEmpty(url))
            {
                if (handler.PlatformView is WebView wv)
                {
                    wv.Source = new Uri(url);
                }
            }
        }
    };

    public HassWebViewHandler() : base(Mapper)
    {

    }

    protected override WebView CreatePlatformView()
    {
        var wv = new WebView();
        // 初始化
        return wv;
    }

    protected override void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        var url = (VirtualView.Source as UrlWebViewSource)?.Url;
        if (!string.IsNullOrEmpty(url))
            platformView.Source = new Uri(url);

        platformView.CoreWebView2Initialized += (s, e) =>
        {
            var core = platformView.CoreWebView2;
            if (core != null)
            {
                core.Settings.AreDefaultContextMenusEnabled = true;
            }
        };
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        // 清理
        base.DisconnectHandler(platformView);
    }
}
