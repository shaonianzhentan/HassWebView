using Android.OS;
using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using Microsoft.Maui.Handlers;

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

    public static CommandMapper commandMapper = new CommandMapper<HassWebView>()
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

    public HassWebViewHandler() : base(propertyMapper, commandMapper)
    {
    }

    protected override WebView CreatePlatformView()
    {
        var webView = new WebView(MauiApplication.Current.ApplicationContext);
        webView.Settings.JavaScriptEnabled = true;
         webView.Settings.JavaScriptCanOpenWindowsAutomatically = false;
        webView.Settings.MixedContentMode = 1; // 总是允许加载图片
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

        webView.WebChromeClient = new WebChromeClientHandler();
        webView.WebViewClient = new WebViewClientHandler(VirtualView);

        //x5object变量非null表示启用x5内核成功
        var x5object = webView.X5WebViewExtension;

        if (x5object != null)
        {
            Console.WriteLine("X5WebViewExtension对象不为null，此为x5webview");
            Bundle data = new Bundle();
            data.PutBoolean("standardFullScreen", false); // true表示标准全屏，false表示X5全屏；不设置默认false，
            data.PutBoolean("supportLiteWnd", false); // false：关闭小窗；true：开启小窗；不设置默认true，
            data.PutInt("DefaultVideoScreen", 2); // 1：以页面内开始播放，2：以全屏开始播放；不设置默认：1
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
        var url = (VirtualView.Source as UrlWebViewSource)?.Url;
        if (!string.IsNullOrEmpty(url))
            platformView.LoadUrl(url);
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        // 停止加载、清理监听器，释放资源
        try
        {
            platformView.StopLoading();
        }
        catch { /* 忽略 */ }

        platformView.WebViewClient = null;
        platformView.WebChromeClient = null;

        base.DisconnectHandler(platformView);
    }
}
