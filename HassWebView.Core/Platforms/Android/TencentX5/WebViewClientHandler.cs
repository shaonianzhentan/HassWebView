using Android.Graphics;
using Com.Tencent.Smtt.Export.External.Interfaces;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Events;

namespace HassWebView.Core.Platforms.Android;


using WebView = Com.Tencent.Smtt.Sdk.WebView;

public class WebViewClientHandler : WebViewClient
{
    private readonly HassWebView _webView;

    public WebViewClientHandler(HassWebView webView)
    {
        _webView = webView;
    }

    public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
    {
        var url = request.Url.ToString();
        var args = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource{ Url = url }, url);
        _webView.SendNavigating(args);

        if (args.Cancel)
        {
            return true;
        }

        view.LoadUrl(url);
        return true;
    }

    public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
    {
        var url = request.Url.ToString();
        var resourceLoadingArgs = new ResourceLoadingEventArgs(url);
        if (_webView.SendResourceLoading(resourceLoadingArgs))
        {
            return new WebResourceResponse(null, null, null);
        }
        return base.ShouldInterceptRequest(view, request);
    }

    public override void OnPageFinished(global::Com.Tencent.Smtt.Sdk.WebView view, string url)
    {
        base.OnPageFinished(view, url);
        _webView.SendNavigated(new WebNavigatedEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Success));
    }

    public override void DoUpdateVisitedHistory(WebView view, string url, bool isReload)
    {
        base.DoUpdateVisitedHistory(view, url, isReload);
        _webView.CanGoBack = view.CanGoBack();
        _webView.CanGoForward = view.CanGoForward();
    }
}