using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using Microsoft.Maui.Handlers;

namespace Maui.HassWebView.Core.Platforms.Android;

using WebView = Com.Tencent.Smtt.Sdk.WebView;

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
                    wv.LoadUrl(url);
                }
            }
        }
    };

    public HassWebViewHandler() : base(Mapper)
    {
        // 在调用TBS初始化、创建WebView之前进行如下配置
        QbSdk.InitTbsSettings(new Dictionary<string, Java.Lang.Object>
        {
            { TbsCoreSettings.TbsSettingsUseSpeedyClassloader, true },
            { TbsCoreSettings.TbsSettingsUseDexloaderService, true },
        });
    }

    protected override WebView CreatePlatformView()
    {
        var webView = new WebView(MauiApplication.Current.ApplicationContext);
        webView.Settings.JavaScriptEnabled = true;
        webView.WebChromeClient = new BasicChrome();
        webView.WebViewClient = new BasicClient();
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

class BasicClient : WebViewClient { }
class BasicChrome : WebChromeClient { }
