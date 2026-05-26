using Foundation;
using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using HassWebView.Core.Models;
using HassWebView.Core.Services;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebKit;

namespace HassWebView.Core.Platforms.iOS;

using WebView = WKWebView;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    private JsBridgeHandler _jsBridgeHandler;
    private string _pendingHtml;
    private string _pendingBaseUrl;

    // --- Manual History Tracking (for GetBackForwardListAsync only) ---
    private readonly List<WebViewHistoryItem> _history = new();
    private int _currentIndex = -1;
    // --------------------------------------------------------------------

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
            wv.CustomUserAgent = view.UserAgent;
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
        [nameof(HassWebView.Focus)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.BecomeFirstResponder();
            }
        },
        [nameof(HassWebView.Unfocus)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.ResignFirstResponder();
            }
        },
        [nameof(HassWebView.EvaluateJavaScriptAsync)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.EvaluateJavaScriptAsyncRequest request) return;
            if (handler.PlatformView is not WebView wv) return;
            try
            {
                var result = await wv.EvaluateJavaScriptAsync(request.Script);
                request.TaskCompletionSource.SetResult(result?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                request.TaskCompletionSource.SetException(ex);
            }
        },
        [nameof(HassWebView.GetBackForwardListAsync)] = (handler, _, args) =>
        {
            if (args is not TaskCompletionSource<WebViewBackForwardList> tcs) return;
            if (handler is not HassWebViewHandler h)
            {
                tcs.SetResult(new WebViewBackForwardList { History = new List<WebViewHistoryItem>(), CurrentIndex = -1 });
                return;
            }

            tcs.SetResult(new WebViewBackForwardList
            {
                History = new List<WebViewHistoryItem>(h._history),
                CurrentIndex = h._currentIndex
            });
        },
        [nameof(HassWebView.SimulateTouch)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView wv) return;
            var scriptTemplate = await ResourceHelper.GetResourceAsync("Scripts/SimulateTouch.js");
            var script = scriptTemplate
                .Replace("__VW__", wv.Frame.Width.ToString(CultureInfo.InvariantCulture))
                .Replace("__VH__", wv.Frame.Height.ToString(CultureInfo.InvariantCulture))
                .Replace("__X__", request.X.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y__", request.Y.ToString(CultureInfo.InvariantCulture));
            await wv.EvaluateJavaScriptAsync(script);
        },
        [nameof(HassWebView.SimulateTouchSlide)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchSlideRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var scriptTemplate = await ResourceHelper.GetResourceAsync("Scripts/SimulateTouchSlide.js");
            var script = scriptTemplate
                .Replace("__VW__", wv.Frame.Width.ToString(CultureInfo.InvariantCulture))
                .Replace("__VH__", wv.Frame.Height.ToString(CultureInfo.InvariantCulture))
                .Replace("__X1__", request.X1.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y1__", request.Y1.ToString(CultureInfo.InvariantCulture))
                .Replace("__X2__", request.X2.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y2__", request.Y2.ToString(CultureInfo.InvariantCulture))
                .Replace("__DURATION__", request.Duration.ToString(CultureInfo.InvariantCulture));
            await wv.EvaluateJavaScriptAsync(script);
        },
        [nameof(HassWebView.ExitFullscreen)] = async (handler, _, args) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            await wv.EvaluateJavaScriptAsync("if (document.fullscreenElement) { document.exitFullscreen(); }");
        }
    };

    public HassWebViewHandler() : base(Mapper, CommandMapper) { }

    protected override WebView CreatePlatformView()
    {
        var configuration = new WKWebViewConfiguration();
        configuration.AllowsInlineMediaPlayback = true;
        configuration.MediaPlaybackRequiresUserAction = false;
        
        // Enable JavaScript
        configuration.Preferences.JavaScriptEnabled = true;
        configuration.Preferences.JavaScriptCanOpenWindowsAutomatically = false;
        
        // Configure web content settings
        configuration.WebsiteDataStore = WKWebsiteDataStore.DefaultDataStore;
        
        var webView = new WebView(CoreGraphics.CGRect.Empty, configuration);
        webView.AutoresizingMask = UIKit.UIViewAutoresizing.FlexibleWidth | UIKit.UIViewAutoresizing.FlexibleHeight;
        
        return webView;
    }

    protected override void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        
        _jsBridgeHandler = new JsBridgeHandler(VirtualView.JsBridges);
        
        // Setup navigation delegate
        platformView.NavigationDelegate = new WebViewNavigationDelegate(this);
        
        // Setup script message handler for JS bridge communication
        var scriptMessageHandler = new ScriptMessageHandler(_jsBridgeHandler);
        platformView.Configuration.UserContentController.AddScriptMessageHandler(scriptMessageHandler, "hassBridge");
        
        // Inject proxy script for JS bridge
        InjectBridgeProxyScript(platformView);
        
        LoadSource(VirtualView.Source);
    }

    private async void InjectBridgeProxyScript(WebView platformView)
    {
        if (_jsBridgeHandler == null) return;
        
        var proxyScript = GenerateiOSProxyScript();
        if (!string.IsNullOrEmpty(proxyScript))
        {
            await platformView.EvaluateJavaScriptAsync(proxyScript);
        }
        
        // Inject the shared injected.js script
        var injectedScript = await ResourceHelper.GetResourceAsync("Scripts/injected.js");
        if (!string.IsNullOrEmpty(injectedScript))
        {
            await platformView.EvaluateJavaScriptAsync(injectedScript);
        }
    }

    private string GenerateiOSProxyScript()
    {
        if (VirtualView?.JsBridges == null || !VirtualView.JsBridges.Any())
            return string.Empty;

        var script = new StringBuilder();
        script.AppendLine("(function() {");
        script.AppendLine("    if (window.hasBridgeProxies) return;");
        script.AppendLine("    window.hasBridgeProxies = true;");

        foreach (var bridgeName in VirtualView.JsBridges.Keys)
        {
            script.AppendLine($@"
    console.log('HassJsBridge: Creating iOS proxy for \'{bridgeName}\'.');
    window['{bridgeName}'] = new Proxy({{}}, {{
        get(target, propKey, receiver) {{
            return (...args) => {{
                const message = {{
                    BridgeName: '{bridgeName}',
                    MethodName: propKey,
                    Arguments: args
                }};
                
                if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.hassBridge) {{
                    window.webkit.messageHandlers.hassBridge.postMessage(message);
                }} else {{
                    console.error(`HassJsBridge: iOS native bridge not found for '{bridgeName}'.`);
                }}
            }};
        }}
    }}); ");
        }

        script.AppendLine("})();");
        return script.ToString();
    }

    private void LoadSource(WebViewSource source)
    {
        if (PlatformView == null)
            return;

        if (source is UrlWebViewSource urlSource && !string.IsNullOrEmpty(urlSource.Url))
        {
            _pendingHtml = null;
            _pendingBaseUrl = null;
            PlatformView.LoadRequest(new NSUrlRequest(new NSUrl(urlSource.Url)));
        }
        else if (source is HtmlWebViewSource htmlSource && !string.IsNullOrEmpty(htmlSource.Html))
        {
            _pendingHtml = htmlSource.Html;
            _pendingBaseUrl = htmlSource.BaseUrl ?? "http://local.html";
            
            if (!string.IsNullOrEmpty(htmlSource.BaseUrl))
            {
                PlatformView.LoadHtmlString(htmlSource.Html, new NSUrl(htmlSource.BaseUrl));
            }
            else
            {
                PlatformView.LoadHtmlString(htmlSource.Html, null);
            }
        }
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        platformView.Configuration.UserContentController.RemoveScriptMessageHandler("hassBridge");
        platformView.NavigationDelegate = null;
        _jsBridgeHandler = null;
        
        // Cleanup history
        _history.Clear();
        _currentIndex = -1;

        base.DisconnectHandler(platformView);
    }

    // Helper method to update navigation state
    private void UpdateNavigationState(WebView webView)
    {
        VirtualView.CanGoBack = webView.CanGoBack;
        VirtualView.CanGoForward = webView.CanGoForward;
    }

    // WKNavigationDelegate implementation
    private class WebViewNavigationDelegate : WKNavigationDelegate
    {
        private readonly HassWebViewHandler _handler;
        
        public WebViewNavigationDelegate(HassWebViewHandler handler)
        {
            _handler = handler;
        }
        
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            var mauiArgs = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, _handler.VirtualView.Source, navigationAction.Request.Url.AbsoluteString);
            _handler.VirtualView.SendNavigating(mauiArgs);
            
            decisionHandler(mauiArgs.Cancel ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
        }
        
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            // Update history tracking
            var currentUrl = webView.Url?.AbsoluteString;
            var currentTitle = webView.Title ?? string.Empty;
            
            if (!string.IsNullOrEmpty(currentUrl))
            {
                // Check if this is a new navigation or back/forward
                bool isNewNavigation = true;
                
                // Check if we're navigating within existing history
                if (_handler._currentIndex >= 0 && _handler._history.Count > _handler._currentIndex)
                {
                    var currentItem = _handler._history[_handler._currentIndex];
                    if (currentItem.Url == currentUrl)
                    {
                        // Same URL, just update title if needed
                        currentItem.Title = currentTitle;
                        isNewNavigation = false;
                    }
                }
                
                if (isNewNavigation)
                {
                    // Remove forward history if we're branching
                    if (_handler._currentIndex < _handler._history.Count - 1)
                    {
                        _handler._history.RemoveRange(_handler._currentIndex + 1, _handler._history.Count - (_handler._currentIndex + 1));
                    }
                    
                    // Add new history item
                    _handler._history.Add(new WebViewHistoryItem { Url = currentUrl, Title = currentTitle });
                    _handler._currentIndex = _handler._history.Count - 1;
                }
            }
            
            // Update CanGoBack/CanGoForward
            _handler.UpdateNavigationState(webView);
            
            // Fire Navigated event
            var mauiArgs = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, _handler.VirtualView.Source, currentUrl, WebNavigationResult.Success);
            _handler.VirtualView.SendNavigated(mauiArgs);
        }
        
        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            _handler.UpdateNavigationState(webView);
            
            var mauiArgs = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, _handler.VirtualView.Source, webView.Url?.AbsoluteString, WebNavigationResult.Failure);
            _handler.VirtualView.SendNavigated(mauiArgs);
        }
        
        public override void DidReceiveServerRedirectForProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            // Handle redirects if needed
        }
        
        public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
        {
            // Navigation committed
        }
    }

    // Script message handler for JS bridge
    private class ScriptMessageHandler : NSObject, IWKScriptMessageHandler
    {
        private readonly JsBridgeHandler _jsBridgeHandler;
        
        public ScriptMessageHandler(JsBridgeHandler jsBridgeHandler)
        {
            _jsBridgeHandler = jsBridgeHandler;
        }
        
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (message.Body is NSDictionary dict)
            {
                // Convert NSDictionary to JSON string
                var jsonData = NSJsonSerialization.Serialize(dict, 0, out var error);
                if (jsonData != null)
                {
                    var jsonString = NSString.FromData(jsonData, NSStringEncoding.UTF8).ToString();
                    _ = _jsBridgeHandler.HandleMessageAsync(jsonString);
                }
            }
            else if (message.Body is NSString str)
            {
                _ = _jsBridgeHandler.HandleMessageAsync(str.ToString());
            }
        }
    }
}