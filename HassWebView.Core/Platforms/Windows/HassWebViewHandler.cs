using HassWebView.Core.Events;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace HassWebView.Core.Platforms.Windows;

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
        },
        [nameof(HassWebView.UserAgent)] = (handler, view) =>
        {
            if (handler.PlatformView is WebView wv && !string.IsNullOrEmpty(view.UserAgent))
            {
                wv.CoreWebView2.Settings.UserAgent = view.UserAgent;
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

        },
        [nameof(HassWebView.SimulateTouchSlide)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchSlideRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var script = $@"
(function(){{
    var vw = {wv.ActualWidth};
    var vh = {wv.ActualHeight};
    var cw = document.documentElement.clientWidth;
    var ch = document.documentElement.clientHeight;

    var x1 = {request.X1} * (cw / vw);
    var y1 = {request.Y1} * (ch / vh);
    var x2 = {request.X2} * (cw / vw);
    var y2 = {request.Y2} * (ch / vh);

    var el = document.elementFromPoint(x1, y1);
    if (el) {{
        const down = new Touch({{ identifier: Date.now(), target: el, clientX: x1, clientY: y1, pageX: x1, pageY: y1, screenX: x1, screenY: y1 }});
        const touchstart = new TouchEvent('touchstart', {{ bubbles: true, cancelable: true, composed: true, detail: 1, view: window, touches: [down], targetTouches: [down], changedTouches: [down] }});
        el.dispatchEvent(touchstart);

        const move = new Touch({{ identifier: Date.now(), target: el, clientX: x2, clientY: y2, pageX: x2, pageY: y2, screenX: x2, screenY: y2 }});
        const touchmove = new TouchEvent('touchmove', {{ bubbles: true, cancelable: true, composed: true, detail: 1, view: window, touches: [move], targetTouches: [move], changedTouches: [move] }});
        el.dispatchEvent(touchmove);

        const up = new Touch({{ identifier: Date.now(), target: el, clientX: x2, clientY: y2, pageX: x2, pageY: y2, screenX: x2, screenY: y2 }});
        const touchend = new TouchEvent('touchend', {{ bubbles: true, cancelable: true, composed: true, detail: 1, view: window, touches: [], targetTouches: [], changedTouches: [up] }});
        el.dispatchEvent(touchend);
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
            core.WebResourceRequested += Core_WebResourceRequested;
        }
    }

    private void Core_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        var mauiArgs = new ResourceLoadingEventArgs(args.Request.Uri);
        if (VirtualView.SendResourceLoading(mauiArgs))
        {
            args.Response = sender.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "");
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
            platformView.CoreWebView2.WebResourceRequested -= Core_WebResourceRequested;

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
