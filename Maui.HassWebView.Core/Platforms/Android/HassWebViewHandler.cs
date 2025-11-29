using Android.OS;
using Com.Tencent.Smtt.Sdk;
using Microsoft.Maui.Handlers;
using System.Collections.Generic;
using Java.Lang;
using Maui.HassWebView.Core.Platforms.Android.TencentX5;
using System.Threading.Tasks;
using Android.Views;
using System;

namespace Maui.HassWebView.Core.Platforms.Android;

using WebView = Com.Tencent.Smtt.Sdk.WebView;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    public static PropertyMapper propertyMapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            var url = (view.Source as UrlWebViewSource)?.Url;
            if (!string.IsNullOrEmpty(url))
            {
                if (handler.PlatformView is WebView wv)
                {
                    wv.LoadUrl(url);
                }
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
        [nameof(HassWebView.SimulateTouch)] = (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView platformView) return;

            int[] location = new int[2];
            platformView.GetLocationOnScreen(location);

            var density = platformView.Resources.DisplayMetrics.Density;
            float x = (float)request.X * density;
            float y = (float)request.Y * density;

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
            float x1 = (float)request.X1 * density;
            float y1 = (float)request.Y1 * density;
            float x2 = (float)request.X2 * density;
            float y2 = (float)request.Y2 * density;
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
        var webView = new WebView(MauiApplication.Current.ApplicationContext);
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

        webView.Settings.SavePassword = true;
        webView.Settings.SaveFormData = true;
        webView.Settings.MediaPlaybackRequiresUserGesture = false;
        webView.Settings.LoadWithOverviewMode = true;
        webView.Settings.UseWideViewPort = true;
        webView.Settings.SetSupportZoom(true);
        webView.Settings.UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";
        webView.Focusable = true;
        webView.FocusableInTouchMode = true;
        webView.Clickable = true;

        webView.WebChromeClient = new WebChromeClientHandler(VirtualView);
        webView.WebViewClient = new WebViewClientHandler(VirtualView);

        var x5object = webView.X5WebViewExtension;
        if (x5object != null)
        {
            Console.WriteLine("X5WebViewExtension对象不为null，此为x5webview");
            Bundle data = new Bundle();
            data.PutBoolean("standardFullScreen", false);
            data.PutBoolean("supportLiteWnd", false);
            data.PutInt("DefaultVideoScreen", 2);
            x5object.InvokeMiscMethod("setVideoParams", data);
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

        var url = (VirtualView.Source as UrlWebViewSource)?.Url;
        if (!string.IsNullOrEmpty(url))
            platformView.LoadUrl(url);
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
}
