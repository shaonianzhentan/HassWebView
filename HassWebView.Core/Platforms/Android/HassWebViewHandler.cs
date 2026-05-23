using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Models; // Add model namespace
using HassWebView.Core.Platforms.Android.TencentX5;
using Java.Interop;
using Java.Lang;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq; // Add for LINQ operations
using System.Threading.Tasks;

namespace HassWebView.Core.Platforms.Android;

using WebView = Com.Tencent.Smtt.Sdk.WebView;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    public static PropertyMapper propertyMapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            if (handler.PlatformView is not WebView wv) return;

            if (view.Source is UrlWebViewSource urlSource && !string.IsNullOrEmpty(urlSource.Url))
            {
                wv.LoadUrl(urlSource.Url);
            }
            else if (view.Source is HtmlWebViewSource htmlSource && !string.IsNullOrEmpty(htmlSource.Html))
            {
                wv.LoadDataWithBaseURL(htmlSource.BaseUrl, htmlSource.Html, "text/html", "UTF-8", null);
            }
        },
        [nameof(HassWebView.UserAgent)] = (handler, view) =>
        {
            if (handler.PlatformView is WebView wv && !string.IsNullOrEmpty(view.UserAgent))
            {
                wv.Settings.UserAgentString = view.UserAgent;
            }
        }
    };

    public static CommandMapper commandMapper = new CommandMapper<HassWebView>
    {
        [nameof(HassWebView.GoBack)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoBack())
            {
                wv.GoBack();
            }
        },
        [nameof(HassWebView.GoForward)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoForward())
            {
                wv.GoForward();
            }
        },
        [nameof(HassWebView.Focus)] = (handler, view, args) =>
        {

            if (handler.PlatformView is WebView wv)
            {
                wv.RequestFocus();
            }
        },
        [nameof(HassWebView.Unfocus)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.ClearFocus();
            }
        },
        [nameof(HassWebView.ExitFullscreen)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.WebChromeClient.OnHideCustomView();
            }
        },
        [nameof(HassWebView.EvaluateJavaScriptAsync)] = (handler, _, args) =>
        {
            if (args is not HassWebView.EvaluateJavaScriptAsyncRequest request) return;
            if (handler.PlatformView is WebView wv)
            {
                wv.EvaluateJavascript(request.Script, new X5ValueCallback((result) =>
                {
                    request.TaskCompletionSource.SetResult(result);
                }));
            }
        },
        // New command mapping for GetBackForwardListAsync
        [nameof(HassWebView.GetBackForwardListAsync)] = (handler, _, args) =>
        {
            if (args is not TaskCompletionSource<WebViewBackForwardList> tcs) return;
            if (handler.PlatformView is not WebView platformView) 
            {
                tcs.SetResult(new WebViewBackForwardList { History = new List<WebViewHistoryItem>() });
                return;
            }

            var nativeList = platformView.CopyBackForwardList();
            if (nativeList == null)
            {
                tcs.SetResult(new WebViewBackForwardList { History = new List<WebViewHistoryItem>() });
                return;
            }

            var historyItems = new List<WebViewHistoryItem>();
            for (int i = 0; i < nativeList.Size; i++)
            {
                var nativeItem = nativeList.GetItemAtIndex(i);
                if (nativeItem != null)
                {
                    historyItems.Add(new WebViewHistoryItem
                    {
                        Url = nativeItem.Url,
                        Title = nativeItem.Title
                    });
                }
            }

            var result = new WebViewBackForwardList
            {
                History = historyItems,
                CurrentIndex = nativeList.CurrentIndex
            };

            tcs.SetResult(result);
        },
        [nameof(HassWebView.SimulateTouch)] = (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView platformView) return;

            int[] location = new int[2];
            platformView.GetLocationOnScreen(location);

            var density = platformView.Resources.DisplayMetrics.Density;
            float x = request.X * density;
            float y = request.Y * density;

            Console.WriteLine($"Cursor = {request.X}, {request.Y}");
            Console.WriteLine($"Device density = {density}");
            Console.WriteLine($"WebView on screen = X:{location[0]}, Y:{location[1]}");
            Console.WriteLine($"WebView size = W:{platformView.Width}, H:{platformView.Height}");
            Console.WriteLine($"Relative = {x}, {y}");

            var downTime = SystemClock.UptimeMillis();
            var eventTime = SystemClock.UptimeMillis();

            var motionEventDown = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Down, x, y, 0);
            platformView.DispatchTouchEvent(motionEventDown);

            var motionEventUp = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Up, x, y, 0);
            platformView.DispatchTouchEvent(motionEventUp);

            motionEventDown.Recycle();
            motionEventUp.Recycle();
        },
        [nameof(HassWebView.SimulateTouchSlide)] = (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchSlideRequest request) return;
            if (handler.PlatformView is not WebView platformView) return;

            var density = platformView.Resources.DisplayMetrics.Density;
            float x1 = request.X1 * density;
            float y1 = request.Y1 * density;
            float x2 = request.X2 * density;
            float y2 = request.Y2 * density;
            int duration = request.Duration;

            var downTime = SystemClock.UptimeMillis();
            var eventTime = SystemClock.UptimeMillis();

            var motionEventDown = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Down, x1, y1, 0);
            platformView.DispatchTouchEvent(motionEventDown);

            int steps = 10;
            float xStep = (x2 - x1) / steps;
            float yStep = (y2 - y1) / steps;
            long stepDuration = (long)duration / steps;

            for (int i = 0; i < steps; i++)
            {
                eventTime += stepDuration;
                float currentX = x1 + xStep * (i + 1);
                float currentY = y1 + yStep * (i + 1);
                var motionEventMove = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Move, currentX, currentY, 0);
                platformView.DispatchTouchEvent(motionEventMove);
                motionEventMove.Recycle();
                SystemClock.Sleep(stepDuration);
            }

            eventTime += stepDuration;
            var motionEventUp = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Up, x2, y2, 0);
            platformView.DispatchTouchEvent(motionEventUp);

            motionEventDown.Recycle();
            motionEventUp.Recycle();
        }
    };

    public HassWebViewHandler() : base(propertyMapper, commandMapper)
    {
    }

    protected override WebView CreatePlatformView()
    {
        var webView = new BackInterceptWebView(MauiApplication.Current.ApplicationContext);
        webView.Settings.JavaScriptEnabled = true;
        webView.Settings.JavaScriptCanOpenWindowsAutomatically = false;
        webView.Settings.MixedContentMode = 1;
        webView.Settings.JavaScriptEnabled = true;
        webView.Settings.DomStorageEnabled = true;
        webView.Settings.AllowContentAccess = true;
        webView.Settings.AllowFileAccess = true;
        webView.Settings.SetAllowFileAccessFromFileURLs(true);
        webView.Settings.SetAllowUniversalAccessFromFileURLs(true);
        webView.Settings.BlockNetworkImage = false;
        webView.Settings.LoadsImagesAutomatically = true;
        webView.Settings.SavePassword = false;
        webView.Settings.SaveFormData = false;
        webView.Settings.MediaPlaybackRequiresUserGesture = false;
        webView.Settings.LoadWithOverviewMode = true;
        webView.Settings.UseWideViewPort = true;
        webView.Settings.SetSupportZoom(true);
        
        webView.Focusable = false;
        webView.FocusableInTouchMode = false;
        webView.Clickable = true;

        webView.WebChromeClient = new WebChromeClientHandler(VirtualView);
        webView.WebViewClient = new WebViewClientHandler(VirtualView);

        webView.SetOnKeyListener(new WebViewKeyListener());

        var x5object = webView.X5WebViewExtension;
        if (x5object != null)
        {
            webView.SettingsExtension.SetContentCacheEnable(true);

            Console.WriteLine("X5WebViewExtension对象不为null，此为x5webview");
            Bundle data = new Bundle();
            data.PutBoolean("standardFullScreen", false);
            data.PutBoolean("supportLiteWnd", false);
            data.PutInt("DefaultVideoScreen", 1);
            x5object.InvokeMiscMethod("setVideoParams", data);

            var sharedPrefs = webView.Context.GetSharedPreferences("tbs_public_settings", FileCreationMode.Private);
            using var editor = sharedPrefs.Edit();
            // 强制开启 EMBEDDED 云控开关
            editor.PutInt("MTT_CORE_EMBEDDED_WIDGET_ENABLE", 1);
            editor.Apply(); // 异步提交（或用 Commit() 同步提交）

            string[] tags = { "hass-video" };
            webView.X5WebViewExtension.RegisterEmbeddedWidget(tags, new WidgetClientFactory(webView));
        }
        else
        {
            Console.WriteLine("X5WebViewExtension对象为null，此为系统自带webview");
        }

        return webView;
    }

    protected override void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView?.JsBridges != null)
        {
            foreach (var bridge in VirtualView.JsBridges)
            {
                if (bridge.Value is Java.Lang.Object javaObject)
                {
                    platformView.AddJavascriptInterface(javaObject, bridge.Key);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[HassWebView] JS Bridge object for '{bridge.Key}' must inherit from Java.Lang.Object.");
                }
            }
        }

        if (VirtualView.Source is UrlWebViewSource urlSource && !string.IsNullOrEmpty(urlSource.Url))
        {
            platformView.LoadUrl(urlSource.Url);
        }
        else if (VirtualView.Source is HtmlWebViewSource htmlSource && !string.IsNullOrEmpty(htmlSource.Html))
        {
            platformView.LoadDataWithBaseURL(htmlSource.BaseUrl, htmlSource.Html, "text/html", "UTF-8", null);
        }
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        try
        {
            platformView.StopLoading();
        }
        catch { /* 忽略 */ }

        platformView.WebViewClient = null;
        platformView.WebChromeClient = null;

        if (VirtualView?.JsBridges != null)
        {
            foreach (var bridge in VirtualView.JsBridges)
            {
                platformView.RemoveJavascriptInterface(bridge.Key);
            }
        }

        base.DisconnectHandler(platformView);
    }

    /// <summary>
    /// 自定义 WebView，重写 onKeyDown 拦截 Back 键，
    /// 阻止 WebView 默认的 GoBack() 行为，统一由 KeyService 体系处理。
    /// </summary>
    private class BackInterceptWebView : WebView
    {
        public BackInterceptWebView(global::Android.Content.Context context) : base(context) { }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            // 拦截 Back 键，阻止 WebView/X5 默认的 GoBack() 行为
            // Back 键的导航返回由 KeyService -> IKeyHandler.OnSingleClick 统一处理
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}
