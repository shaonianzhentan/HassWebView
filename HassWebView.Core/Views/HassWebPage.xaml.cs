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
    private readonly IRemoteControlService _remoteControlService; // 修正：注入遥控服务
    private readonly HassPageOptions _pageOptions;
    private string _defaultUserAgent;

    public string Url { get; set; }

    // 构造函数已修正
    public HassWebPage(HassPageOptions pageOptions, IRemoteControlService remoteControlService, HttpServer httpServer = null)
    {
        InitializeComponent();

        _pageOptions = pageOptions;
        _remoteControlService = remoteControlService; // 修正：保存遥控服务实例
        _httpServer = httpServer;

        var wv = webView.WebViewControl;

        wv.Navigating += OnWebViewNavigating;
        wv.Navigated += OnWebViewNavigated;
        wv.ResourceLoading += OnWebViewResourceLoading;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;

        // 新增：默认显示光标
        webView.CursorControl.IsVisible = true;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (!string.IsNullOrEmpty(Url))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                webView.WebViewControl.Source = new UrlWebViewSource { Url = Url };
            });
        }
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var wv = webView.WebViewControl;
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
        var wv = webView.WebViewControl;
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
                string escapedCss = config.Css.Replace("'", "\'").Replace("`", "\\`").Replace("$", "\\$");
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
        var wv = webView.WebViewControl;
        var urlString = e.Url.ToString();
        Debug.WriteLine($"ResourceLoading：{urlString}");
        if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) ||
             urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
        {
            await ResourceHelper.ExecuteScriptAsync(wv, "Scripts/VideoPanel.js", $"HassVideoPanel.add('{urlString}');");
        }
    }

    private void OnExternalBusMessageReceived(object sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        try
        {
            var msg = JsonNode.Parse(message);
            var type = msg?["type"]?.GetValue<string>();
            switch (type)
            {
                case "video/play":
                    var videoUrl = msg?["data"]?.GetValue<string>();
                    var origin = msg?["origin"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(videoUrl)) _pageOptions.PlayVideo(videoUrl, origin, false);
                    break;
                case "play/video":
                    var videoUrl2 = msg?["data"]?.GetValue<string>();
                    _pageOptions.PlayVideo(videoUrl2, null, false);
                    break;
            }
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[ExternalBus] Error parsing JSON: {ex.Message}");
        }
    }

    // OnAppearing 已修正
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _remoteControlService.SetActiveControl(webView); // 新增：注册按键处理器
    }

    // OnDisappearing 已修正
    protected override void OnDisappearing()
    {
        _remoteControlService.ClearActiveControl(webView); // 新增：注销按键处理器
        base.OnDisappearing();
    }


    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    // OnSingleClick 已修正，包含智能返回逻辑
    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        if (args.KeyName == "Back")
        {
            if (webView.WebViewControl.CanGoBack)
            {
                webView.WebViewControl.GoBack();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PopModalAsync());
            }
        }
        else
        {
            webView.OnSingleClick(args.KeyName);
        }
    }

    // OnDoubleClick 保持原样
    public void OnDoubleClick(RemoteKeyEventArgs args)
    {
        webView.OnDoubleClick(args.KeyName);
    }

    // OnLongClick 保持原样
    public void OnLongClick(RemoteKeyEventArgs args)
    {
        if (webView.OnLongClick(args.KeyName)) return;

        if (args.KeyName == "Back")
        {
            MainThread.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PopModalAsync());
        }
    }

    #endregion
}