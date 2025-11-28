using Microsoft.Maui.Handlers;
using Microsoft.Web.WebView2.Core;
using System;

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

    public static CommandMapper CommandMapper = new CommandMapper<HassWebView>()
    {
        [nameof(HassWebView.GoBack)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.GoBack();
            }
        },
        [nameof(HassWebView.GoForward)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.GoForward();
            }
        }
    };

    public HassWebViewHandler() : base(Mapper, CommandMapper)
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

        platformView.CoreWebView2Initialized += CoreWebView2Initialized;
    }

    private void CoreWebView2Initialized(WebView sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
    {
        var core = sender.CoreWebView2;
        if (core != null)
        {
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.NavigationStarting += Core_NavigationStarting;
            core.NavigationCompleted += Core_NavigationCompleted;
        }
    }

    private void Core_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        var mauiArgs = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, VirtualView.Source, args.Uri);
        VirtualView.SendNavigating(mauiArgs);
        args.Cancel = mauiArgs.Cancel;
    }

    private void Core_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        var state = args.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;
        var mauiArgs = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, VirtualView.Source, sender.Source, state);
        VirtualView.SendNavigated(mauiArgs);
        VirtualView.SetValue(global::Microsoft.Maui.Controls.WebView.CanGoBackProperty, sender.CanGoBack);
        VirtualView.SetValue(global::Microsoft.Maui.Controls.WebView.CanGoForwardProperty, sender.CanGoForward);
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        // 清理
        if (platformView.CoreWebView2 != null)
        {
            platformView.CoreWebView2.NavigationStarting -= Core_NavigationStarting;
            platformView.CoreWebView2.NavigationCompleted -= Core_NavigationCompleted;
        }
        platformView.CoreWebView2Initialized -= CoreWebView2Initialized;
        base.DisconnectHandler(platformView);
    }
}
