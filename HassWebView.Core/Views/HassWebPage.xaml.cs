using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace HassWebView.Core.Views;

public partial class HassWebPage : ContentPage, IKeyHandler
{
    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;
    private readonly HassPageOptions _pageOptions;
    private readonly CursorControl _cursorControl;
    private string _defaultUserAgent;

    public string Url { get; set; }

    public HassWebPage(HassPageOptions pageOptions, KeyService keyService = null, HttpServer httpServer = null)
    {
        InitializeComponent();

        var wv = webView.WebView;
        var cursor = webView.Cursor;
        var root = webView.Root;

        _ = LoadEmbeddedHtml("loading.html");

        _pageOptions = pageOptions;
        _keyService = keyService;
        _httpServer = httpServer;

        _pageOptions.PlayVideo = DisplayVideoPlayer;
        _pageOptions.SetWebViewSource = (newSource) => MainThread.BeginInvokeOnMainThread(() => wv.Source = newSource);

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
        }

        if (_httpServer != null)
        {
            _httpServer.Get("/webview/remote", async (req, res) =>
            {
                var htmlContent = await ResourceHelper.GetResourceAsync("remote.html");
                await res.Html(htmlContent);
            });

            _httpServer.Get("/webview/config", async (req, res) =>
            {
                await res.Json(new { width = wv.Width });
            });

            _httpServer.Post("/webview/remote", async (req, res) =>
            {
                var query = HttpUtility.ParseQueryString(await req.BodyAsync());
                var type = query["type"];

                switch (type)
                {
                    case "move":
                        _cursorControl.MoveBy(Convert.ToDouble(query["x"]), Convert.ToDouble(query["y"]));
                        break;
                    case "click":
                        _cursorControl.Click();
                        break;
                    case "text":
                        var append = query["append"] == "1";
                        var text = query["text"];
                        await ResourceHelper.ExecuteScriptAsync(wv, "Scripts/TextInput.js", $"HassTextInput.insert('{text.Replace("\'", "\\\'")}', {append.ToString().ToLower()});");
                        break;
                }
                await res.Text("");
            });
        }

        wv.Navigating += OnWebViewNavigating;
        wv.Navigated += OnWebViewNavigated;
        wv.ResourceLoading += OnWebViewResourceLoading;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (!string.IsNullOrEmpty(Url))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                webView.WebView.Source = new UrlWebViewSource { Url = Url };
            });
        }
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var wv = webView.WebView;
        Debug.WriteLine($"[HassWebPage] Navigating to: {e.Url}");

        if (string.IsNullOrEmpty(_defaultUserAgent) && !string.IsNullOrEmpty(wv.UserAgent))
        {
            _defaultUserAgent = wv.UserAgent;
            Debug.WriteLine($"[HassWebPage] Default User-Agent captured: {_defaultUserAgent}");
        }

        try
        {
            var uri = new Uri(e.Url);
            var host = uri.Host;

            var config = _pageOptions.DomainConfigs?
                .FirstOrDefault(kvp => host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                .Value;

            string targetUserAgent = (config != null && !string.IsNullOrWhiteSpace(config.UserAgent))
                ? config.UserAgent
                : _defaultUserAgent;

            if (wv.UserAgent != targetUserAgent && !string.IsNullOrEmpty(targetUserAgent))
            {
                Debug.WriteLine($"[HassWebPage] Applying User-Agent for {host}: {targetUserAgent}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    wv.UserAgent = targetUserAgent;
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassWebPage] Error applying User-Agent: {ex.Message}");
        }
    }

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        var wv = webView.WebView;
        if (e.Result != WebNavigationResult.Success || e.Source is not UrlWebViewSource urlSource) return;
        Debug.WriteLine($"[HassWebPage] Navigated to: {urlSource.Url}");

        try
        {
            var uri = new Uri(urlSource.Url);
            var host = uri.Host;

            var config = _pageOptions.DomainConfigs?
                .FirstOrDefault(kvp => host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                .Value;

            if (config == null) return;

            if (!string.IsNullOrWhiteSpace(config.Css))
            {
                string escapedCss = config.Css.Replace("\'", "\\\'").Replace("`", "\`").Replace("$", "\$");
                await ResourceHelper.ExecuteScriptAsync(wv, "Scripts/CssInjector.js", $"HassCssInjector.inject(`{escapedCss}`, '{host}');");
            }

            if (!string.IsNullOrWhiteSpace(config.Js))
            {
                await wv.EvaluateJavaScriptAsync(config.Js);
                Debug.WriteLine($"[HassWebView] Executed custom JS for {host}.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassWebPage] Error applying domain config (CSS/JS): {ex.Message}");
        }
    }

    private async void OnWebViewResourceLoading(object sender, ResourceLoadingEventArgs e)
    {
        var wv = webView.WebView;
        var urlString = e.Url.ToString();
        Debug.WriteLine($"ResourceLoading：{urlString}");
        if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) ||
             urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
        {
            await ResourceHelper.ExecuteScriptAsync(wv, "Scripts/VideoPanel.js", $"HassVideoPanel.add('{urlString}');");
        }
    }

    private async void OnExternalBusMessageReceived(object sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        var wv = webView.WebView;

        try
        {
            var msg = JsonNode.Parse(message);
            var type = msg?["type"]?.GetValue<string>();
            switch (type)
            {
                case "video/play":
                    var videoUrl = msg?["data"]?.GetValue<string>();
                    var origin = msg?["origin"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(videoUrl)) await DisplayVideoPlayer(videoUrl, origin);
                    break;
                case "play/video":
                    var videoUrl2 = msg?["data"]?.GetValue<string>();
#if ANDROID
    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
    intent.SetDataAndType(Android.Net.Uri.Parse(videoUrl2), "video/*");
    intent.SetFlags(Android.Content.ActivityFlags.NewTask);
    Android.App.Application.Context.StartActivity(intent);
#else
    Launcher.Default.OpenAsync(new Uri(videoUrl2));
#endif
                    break;
#if ANDROID
                case "x5/init":
                    string apkUrl = string.Empty;
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) apkUrl = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm64_046295.tbs.apk";
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm) apkUrl = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm_045912_x5.tbs.apk";
                    if (!string.IsNullOrEmpty(apkUrl))
                    {
                        Debug.WriteLine($"[ExternalBus] Initializing Tencent X5 Core with APK: {apkUrl}");
                        var result = await TencentX5Service.InitializeX5CoreAsync(apkUrl, (progress) => {
                            wv.WindowExternalBus(new { type = "x5/download", data = progress });
                        });
                        if (result) wv.WindowExternalBus(new { type = "x5/init" });
                    }
                    break;
#endif
            }
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[ExternalBus] Error parsing JSON: {ex.Message}");
        }
    }

    private Task DisplayVideoPlayer(string url, string? baseUrl)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var mediaPage = new HassMediaPage { Url = url };

            if(!string.IsNullOrEmpty(baseUrl)){
                var uri = new Uri(baseUrl);
                var host = uri.Host;

                var config = _pageOptions.DomainConfigs?
                    .FirstOrDefault(kvp => host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    .Value;

                if (config != null){
                    mediaPage.BaseUrl = config.Referer;
                }
            }

            return Shell.Current.Navigation.PushModalAsync(mediaPage, true);
        });
    }

    private async Task LoadEmbeddedHtml(string resourcePath)
    {
        var wv = webView.WebView;
        try
        {
            var htmlContent = await ResourceHelper.GetResourceAsync(resourcePath);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                wv.Source = new HtmlWebViewSource
                {
                    Html = htmlContent
                };
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassWebPage] Error loading embedded HTML: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                wv.Source = new HtmlWebViewSource { Html = "<h1>Error: Embedded resource not found.</h1>" };
            });
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        _keyService?.StopRepeatingAction();
        base.OnDisappearing();
    }


    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        webView.OnSingleClick(args.KeyName);
    }

    public void OnDoubleClick(RemoteKeyEventArgs args)
    {
        webView.OnSingleClick(args.KeyName);
    }

    public async void OnLongClick(RemoteKeyEventArgs args)
    {
        var result = webView.OnSingleClick(args.KeyName);
        if (!result)
        {
            if (args.KeyName == "Back")
            {
                Shell.Current.Navigation.PopModalAsync();
            }
        }
    }

    #endregion

}
