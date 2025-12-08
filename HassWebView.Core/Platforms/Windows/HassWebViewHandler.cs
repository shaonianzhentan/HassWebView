using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HassWebView.Core.Platforms.Windows;

using WebView = Microsoft.UI.Xaml.Controls.WebView2;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    private JsBridgeHandler _jsBridgeHandler;
    private string _pendingHtml;
    private string _pendingBaseUrl;

    public static PropertyMapper Mapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            if (handler is not HassWebViewHandler h) return;
            h.LoadSource(view.Source);
        },
        [nameof(HassWebView.UserAgent)] = (handler, view) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            if (string.IsNullOrEmpty(view.UserAgent)) return;
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
        [nameof(HassWebView.SimulateTouch)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView wv) return;
            var script = $@"(function(){{
    var vw = {wv.ActualWidth}; var vh = {wv.ActualHeight};
    var cw = document.documentElement.clientWidth; var ch = document.documentElement.clientHeight;
    var x = {request.X} * (cw/vw); var y = {request.Y} * (ch/vh);
    var el = document.elementFromPoint(x, y);
    if (el) {{
        el.scrollIntoView({{block:'center',inline:'center'}});
        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') el.focus();
        else {{
            el.dispatchEvent(new Event('touchstart',{{bubbles:true,cancelable:true}}));
            el.dispatchEvent(new Event('touchend',{{bubbles:true,cancelable:true}}));
            el.dispatchEvent(new MouseEvent('click',{{bubbles:true,cancelable:true,view:window}}));
        }}
    }}
}})();";
            await wv.ExecuteScriptAsync(script);
        },
        [nameof(HassWebView.SimulateTouchSlide)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchSlideRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var script = $@"(function(){{
                var vw = {wv.ActualWidth}; var vh = {wv.ActualHeight};
                var cw = document.documentElement.clientWidth; var ch = document.documentElement.clientHeight;
                var x1 = {request.X1} * (cw/vw); var y1 = {request.Y1} * (ch/vh);
                var x2 = {request.X2} * (cw/vw); var y2 = {request.Y2} * (ch/vh);
                var duration = {request.Duration};

                var el = document.elementFromPoint(x1, y1);
                if (!el) return;

                const createTouch = (x, y) => new Touch({{
                    identifier: Date.now(), target: el, clientX: x, clientY: y, pageX: x, pageY: y
                }});

                const dispatchTouchEvent = (type, touches) => el.dispatchEvent(new TouchEvent(type, {{
                    bubbles: true, cancelable: true, view: window, touches: touches, targetTouches: touches, changedTouches: touches
                }}));

                dispatchTouchEvent('touchstart', [createTouch(x1, y1)]);
                
                let startTime = performance.now();
                function animate(currentTime) {{
                    let elapsedTime = currentTime - startTime;
                    if (elapsedTime >= duration) {{
                        dispatchTouchEvent('touchmove', [createTouch(x2, y2)]);
                        dispatchTouchEvent('touchend', [createTouch(x2, y2)]);
                        return;
                    }}
                    let progress = elapsedTime / duration;
                    let currentX = x1 + (x2 - x1) * progress;
                    let currentY = y1 + (y2 - y1) * progress;
                    dispatchTouchEvent('touchmove', [createTouch(currentX, currentY)]);
                    requestAnimationFrame(animate);
                }}
                requestAnimationFrame(animate);
            }})();";
            await wv.ExecuteScriptAsync(script);
        },
        [nameof(HassWebView.ExitFullscreen)] = async (handler, _, args) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            await wv.ExecuteScriptAsync("if (document.fullscreenElement) { document.exitFullscreen(); }");
        }
    };

    public HassWebViewHandler() : base(Mapper, CommandMapper) { }

    protected override WebView CreatePlatformView()
    {
        return new WebView();
    }

    protected override async void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        _jsBridgeHandler = new JsBridgeHandler(VirtualView.JsBridges);
        platformView.WebMessageReceived += PlatformView_WebMessageReceived;
        platformView.CoreWebView2Initialized += PlatformView_CoreWebView2Initialized;
        await platformView.EnsureCoreWebView2Async();
    }

    private async void PlatformView_WebMessageReceived(WebView sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (_jsBridgeHandler != null)
        {
            await _jsBridgeHandler.HandleMessageAsync(args.WebMessageAsJson);
        }
    }

    private async void PlatformView_CoreWebView2Initialized(WebView sender, CoreWebView2InitializedEventArgs args)
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

        if (_jsBridgeHandler != null)
        {
            var proxyScript = _jsBridgeHandler.GenerateProxyScript();
            if (!string.IsNullOrEmpty(proxyScript))
            {
                await core.AddScriptToExecuteOnDocumentCreatedAsync(proxyScript);
            }
        }
        LoadSource(VirtualView.Source);
    }

    private void Core_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        if (!string.IsNullOrEmpty(_pendingBaseUrl) && args.Request.Uri == _pendingBaseUrl)
        {
            var htmlBytes = Encoding.UTF8.GetBytes(_pendingHtml);
            var memoryStream = new MemoryStream(htmlBytes);
            var randomAccessStream = new InMemoryRandomAccessStream();
            using (var dataWriter = new DataWriter(randomAccessStream))
            {
                dataWriter.WriteBytes(htmlBytes);
                dataWriter.StoreAsync().GetAwaiter().GetResult();
                randomAccessStream.Seek(0);
            }

            var response = sender.Environment.CreateWebResourceResponse(
                randomAccessStream,
                200, "OK", "Content-Type: text/html; charset=utf-8");
            args.Response = response;
            _pendingHtml = null;
            _pendingBaseUrl = null;
            return;
        }

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

    void LoadSource(WebViewSource source)
    {
        if (PlatformView?.CoreWebView2 == null)
            return;

        if (source is UrlWebViewSource urlSource)
        {
            _pendingHtml = null;
            _pendingBaseUrl = null;
            PlatformView.CoreWebView2.Navigate(urlSource.Url);
        }
        else if (source is HtmlWebViewSource htmlSource)
        {
            _pendingHtml = htmlSource.Html;
            _pendingBaseUrl = htmlSource.BaseUrl ?? "http://local.html";
            PlatformView.CoreWebView2.Navigate(_pendingBaseUrl);
        }
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
        }
        _jsBridgeHandler = null;
        base.DisconnectHandler(platformView);
    }
}
