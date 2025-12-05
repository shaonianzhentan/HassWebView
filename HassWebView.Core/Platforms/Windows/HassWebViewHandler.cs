using HassWebView.Core.Events;
using HassWebView.Core.Models;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace HassWebView.Core.Platforms.Windows;

using WebView = Microsoft.UI.Xaml.Controls.WebView2;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    public static PropertyMapper Mapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            if (handler.PlatformView is not WebView wv) return;

            var url = (view.Source as UrlWebViewSource)?.Url;

            // 在 CoreWebView2 初始化后才可以设置 Source
            if (!string.IsNullOrEmpty(url) && wv.CoreWebView2 != null)
            {
                wv.Source = new Uri(url);
            }
        },

        [nameof(HassWebView.UserAgent)] = (handler, view) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            if (string.IsNullOrEmpty(view.UserAgent)) return;

            // CoreWebView2 尚未初始化，先缓存，等 ConnectHandler 设置
            if (wv.CoreWebView2 != null)
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
                wv.GoBack();
        },

        [nameof(HassWebView.GoForward)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoForward)
                wv.GoForward();
        },

        [nameof(HassWebView.EvaluateJavaScriptAsync)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.EvaluateJavaScriptAsyncRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            try
            {
                var result = await wv.ExecuteScriptAsync(request.Script);
                request.TaskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                request.TaskCompletionSource.SetException(ex);
            }
        },

        // 保留你的触摸模拟逻辑
        [nameof(HassWebView.SimulateTouch)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var script = $@"
(function(){{
    var vw = {wv.ActualWidth}; var vh = {wv.ActualHeight};
    var cw = document.documentElement.clientWidth; var ch = document.documentElement.clientHeight;

    var x = {request.X} * (cw / vw);
    var y = {request.Y} * (ch / vh);

    var el = document.elementFromPoint(x, y);
    if (el) {{
        el.scrollIntoView({{block:'center', inline:'center'}});
        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') el.focus();
        else {{
            el.dispatchEvent(new Event('touchstart', {{bubbles:true,cancelable:true}}));
            el.dispatchEvent(new Event('touchend', {{bubbles:true,cancelable:true}}));
            el.dispatchEvent(new MouseEvent('click', {{bubbles:true,cancelable:true,view:window}}));
        }}
    }}
}})();";

            await wv.ExecuteScriptAsync(script);
        }
    };

    public HassWebViewHandler() : base(Mapper, CommandMapper) { }

    protected override WebView CreatePlatformView()
    {
        var wv = new WebView();
        return wv;
    }


    protected override async void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.WebMessageReceived += PlatformView_WebMessageReceived;
        platformView.CoreWebView2Initialized += PlatformView_CoreWebView2Initialized;
        await platformView.EnsureCoreWebView2Async();

    }

    private void PlatformView_WebMessageReceived(WebView sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        // 处理来自 JavaScript 的消息
        var jsMethod = JsonSerializer.Deserialize<JsMethod>(args.WebMessageAsJson);
        if (jsMethod != null)
        {
            // 触发事件
        }
    }

    private async void PlatformView_CoreWebView2Initialized(WebView sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
    {
        var core = sender.CoreWebView2;
        if (!string.IsNullOrEmpty(VirtualView.UserAgent))
        {
            core.Settings.UserAgent = VirtualView.UserAgent;
        }
        core.Settings.IsWebMessageEnabled = true;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.NavigationStarting += Core_NavigationStarting;
        core.NavigationCompleted += Core_NavigationCompleted;
        core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += Core_WebResourceRequested;
        await core.AddScriptToExecuteOnDocumentCreatedAsync(@"
window.HassJsBridge = new Proxy({}, {
    get(target, propKey) {
        return (...args) => {
            window.chrome.webview.postMessage({
                name: propKey,
                arguments: args
            });
        };
    }
});
");
        // 设置初始 URL
        var url = (VirtualView.Source as UrlWebViewSource)?.Url;
        if (!string.IsNullOrEmpty(url))
        {
            sender.Source = new Uri(url);
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
        var result = args.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;
        var mauiArgs = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, VirtualView.Source, sender.Source, result);
        VirtualView.SendNavigated(mauiArgs);

        VirtualView.CanGoBack = sender.CanGoBack;
        VirtualView.CanGoForward = sender.CanGoForward;
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        platformView.WebMessageReceived -= PlatformView_WebMessageReceived;
        platformView.CoreWebView2Initialized -= PlatformView_CoreWebView2Initialized;
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
                    catch { }
                }
            }
        }

        base.DisconnectHandler(platformView);
    }
}
