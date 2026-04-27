using Android.Graphics;
using Com.Tencent.Smtt.Export.External.Interfaces;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Events;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace HassWebView.Core.Platforms.Android;

using WebView = Com.Tencent.Smtt.Sdk.WebView;

public class WebViewClientHandler : WebViewClient
{
    private readonly HassWebView _webView;
    private static readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = false });

    public WebViewClientHandler(HassWebView webView)
    {
        _webView = webView;
    }

    // Centralized navigation handling logic.
    private bool HandleShouldOverrideUrlLoading(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Fire the .NET Navigating event to let the user code decide.
        var args = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url);
        _webView.SendNavigating(args);

        // If the .NET event was canceled, return true to stop the WebView from navigating.
        if (args.Cancel)
        {
            Debug.WriteLine($"[WebViewClient] Navigation to {url} cancelled by user logic.");
            return true;
        }

        // Return false to let the WebView handle the navigation.
        return false;
    }

    // Override the modern version of ShouldOverrideUrlLoading.
    public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
    {
        return HandleShouldOverrideUrlLoading(request?.Url?.ToString());
    }

    // Override the deprecated version of ShouldOverrideUrlLoading to ensure compatibility.
    public override bool ShouldOverrideUrlLoading(WebView view, string url)
    {
        return HandleShouldOverrideUrlLoading(url);
    }
    
    public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
    {
        var url = request.Url.ToString();

        var resourceLoadingArgs = new ResourceLoadingEventArgs(url);
        if (_webView.SendResourceLoading(resourceLoadingArgs))
        {
            return new WebResourceResponse(null, null, null);
        }

        var acceptHeader = request.RequestHeaders.ContainsKey("Accept") ? request.RequestHeaders["Accept"] : "";
        if (request.IsForMainFrame || (acceptHeader != null && acceptHeader.Contains("text/html")))
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                foreach (var header in request.RequestHeaders)
                {
                    if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) &&
                        !header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var httpResponse = _httpClient.Send(httpRequest, HttpCompletionOption.ResponseHeadersRead);

                var mimeType = httpResponse.Content.Headers.ContentType?.MediaType ?? "text/html";
                var encoding = httpResponse.Content.Headers.ContentType?.CharSet ?? "UTF-8";
                var statusCode = (int)httpResponse.StatusCode;
                var reasonPhrase = httpResponse.ReasonPhrase ?? "OK";
                var dataStream = httpResponse.Content.ReadAsStream();

                var responseHeaders = new Dictionary<string, string>();
                var allHeaders = httpResponse.Headers.Concat(httpResponse.Content.Headers);

                foreach (var header in allHeaders)
                {
                    if (header.Key.Equals("X-Frame-Options", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Content-Security-Policy", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[WebView] Stripped header: {header.Key} for URL: {url}");
                        continue;
                    }
                    responseHeaders[header.Key] = string.Join(", ", header.Value);
                }

                if (statusCode >= 300 && statusCode < 400 && !responseHeaders.ContainsKey("Location") && httpResponse.Headers.Location != null)
                {
                    responseHeaders["Location"] = httpResponse.Headers.Location.ToString();
                }

                return new WebResourceResponse(mimeType, encoding, statusCode, reasonPhrase, responseHeaders, dataStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebView] Failed to intercept request for {url}: {ex.Message}");
                return base.ShouldInterceptRequest(view, request);
            }
        }
        return base.ShouldInterceptRequest(view, request);
    }

    public override void OnReceivedHttpError(WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
    {
        base.OnReceivedHttpError(view, request, errorResponse);
    }

    public override void OnPageStarted(WebView view, string url, Bitmap p2)
    {
        base.OnPageStarted(view, url, p2);
    }

    public override void OnPageFinished(WebView view, string url)
    {
        base.OnPageFinished(view, url);
        _webView.SendNavigated(new WebNavigatedEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Success));
    }

    public override async void DoUpdateVisitedHistory(WebView view, string url, bool isReload)
    {
        base.DoUpdateVisitedHistory(view, url, isReload);
        var list = await _webView.GetBackForwardListAsync();
        _webView.CanGoBack = list.CurrentIndex > 0;
        _webView.CanGoForward = list.CurrentIndex < list.History.Count - 1;
    }

    public override void OnReceivedSslError(WebView p0, ISslErrorHandler p1, ISslError p2)
    {
        p1.Proceed();
    }
}
