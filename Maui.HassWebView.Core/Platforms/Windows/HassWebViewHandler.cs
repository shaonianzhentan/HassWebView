using Microsoft.Maui.Handlers;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

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

    public static CommandMapper CommandMapper = new CommandMapper<HassWebView>
    {
        [nameof(HassWebView.GoBack)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoBack)
            {
                wv.GoBack();
            }
        },
        [nameof(HassWebView.GoForward)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoForward)
            {
                wv.GoForward();
            }
        },
        [nameof(HassWebView.EvaluateJavaScriptAsync)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.EvaluateJavaScriptAsyncRequest request) return;
            try
            {
                if (handler.PlatformView is WebView wv)
                {
                    var result = await wv.ExecuteScriptAsync(request.Script);
                    request.TaskCompletionSource.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                request.TaskCompletionSource.SetException(ex);
            }
        },
        [nameof(HassWebView.SimulateTouch)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var script = $@"
(function(){{
    var vw = {wv.ActualWidth};
    var vh = {wv.ActualHeight};
    var cw = document.documentElement.clientWidth;
    var ch = document.documentElement.clientHeight;

    var x = {request.X} * (cw / vw);
    var y = {request.Y} * (ch / vh);

    var el = document.elementFromPoint(x, y);
    if (el) {{
        el.scrollIntoView({{block:'center', inline:'center'}});
        if(el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {{
            el.focus();
        }} else {{
            const ts = new Event('touchstart', {{bubbles:true,cancelable:true}});
            el.dispatchEvent(ts);
            const te = new Event('touchend', {{bubbles:true,cancelable:true}});
            el.dispatchEvent(te);
            const click = new MouseEvent('click', {{bubbles:true,cancelable:true,view:window}});
            el.dispatchEvent(click);
        }}
    }}
}})();";

            await wv.ExecuteScriptAsync(script);

        }

    };

    public HassWebViewHandler() : base(Mapper, CommandMapper)
    {

    }

    protected override WebView CreatePlatformView()
    {
        var wv = new WebView();
        return wv;
    }

    protected override async void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        await platformView.EnsureCoreWebView2Async();

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
            if (VirtualView?.JsBridges != null)
            {
                foreach (var bridge in VirtualView.JsBridges)
                {
                    core.AddHostObjectToScript(bridge.Key, bridge.Value);
                }
            }

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
        VirtualView.CanGoBack = sender.CanGoBack;
        VirtualView.CanGoForward = sender.CanGoForward;
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        if (platformView.CoreWebView2 != null)
        {
            platformView.CoreWebView2.NavigationStarting -= Core_NavigationStarting;
            platformView.CoreWebView2.NavigationCompleted -= Core_NavigationCompleted;

            if (VirtualView?.JsBridges != null)
            {
                foreach (var bridge in VirtualView.JsBridges)
                {
                    try
                    {
                        platformView.CoreWebView2.RemoveHostObjectFromScript(bridge.Key);
                    }
                    catch { /* ignore */ }
                }
            }
        }
        platformView.CoreWebView2Initialized -= CoreWebView2Initialized;
        base.DisconnectHandler(platformView);
    }
}