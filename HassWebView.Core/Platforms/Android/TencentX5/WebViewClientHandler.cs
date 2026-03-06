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
    // Use a static HttpClient for performance and to avoid socket exhaustion.
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = false // Let the WebView handle redirects internally.
    });

    public WebViewClientHandler(HassWebView webView)
    {
        _webView = webView;
    }

    public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
    {
        var url = request.Url.ToString();

        // First, run the existing resource loading logic to potentially block ads or trackers.
        var resourceLoadingArgs = new ResourceLoadingEventArgs(url);
        if (_webView.SendResourceLoading(resourceLoadingArgs))
        {
            // Block the request by returning an empty response.
            return new WebResourceResponse(null, null, null);
        }

        // The core logic to strip headers. We only do this for HTML documents.
        var acceptHeader = request.RequestHeaders.ContainsKey("Accept") ? request.RequestHeaders["Accept"] : "";
        if (request.IsForMainFrame || (acceptHeader != null && acceptHeader.Contains("text/html")))
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

                // Copy most headers from the original WebView request to our new HttpClient request.
                foreach (var header in request.RequestHeaders)
                {
                    if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) &&
                        !header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Execute the request and wait for the response headers.
                // This is a blocking call, but it's safe because ShouldInterceptRequest runs on a background thread.
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
                    // This is where we filter out the problematic headers.
                    if (header.Key.Equals("X-Frame-Options", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Content-Security-Policy", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[WebView] Stripped header: {header.Key} for URL: {url}");
                        continue;
                    }
                    responseHeaders[header.Key] = string.Join(", ", header.Value);
                }

                // For redirects (3xx), ensure the Location header is present for the WebView to follow it.
                if (statusCode >= 300 && statusCode < 400 && !responseHeaders.ContainsKey("Location") && httpResponse.Headers.Location != null)
                {
                    responseHeaders["Location"] = httpResponse.Headers.Location.ToString();
                }

                // Return the new response with the filtered headers to the WebView.
                return new WebResourceResponse(mimeType, encoding, statusCode, reasonPhrase, responseHeaders, dataStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebView] Failed to intercept request for {url}: {ex.Message}");
                // Fallback to default behavior on any error.
                return base.ShouldInterceptRequest(view, request);
            }
        }

        // For all other resource types (images, CSS, JS, etc.), use the default WebView behavior.
        return base.ShouldInterceptRequest(view, request);
    }

    // onReceivedHttpError is useful for logging, but not for changing the response.
    public override void OnReceivedHttpError(WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
    {
        base.OnReceivedHttpError(view, request, errorResponse);
    }

    public override void OnPageStarted(WebView view, string url, Bitmap p2)
    {
        var args = new WebNavigatingEventArgs(
            WebNavigationEvent.NewPage,
            new UrlWebViewSource { Url = url },
            url);

        _webView.SendNavigating(args);

        if (args.Cancel)
        {
            view.StopLoading();
            return;
        }

        base.OnPageStarted(view, url, p2);
    }

    public override void OnPageFinished(WebView view, string url)
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

    public override void OnReceivedSslError(WebView p0, ISslErrorHandler p1, ISslError p2)
    {
        // This is not recommended for production apps, but it is often necessary for
        // self-signed certificates used by local Home Assistant instances.
        p1.Proceed();
    }
}
