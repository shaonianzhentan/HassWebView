using HassWebView.Core.Auth;
using HassWebView.Core.Configuration;
using HassWebView.Core.Interfaces;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Web;
using System.Runtime.InteropServices;
using HassWebView.Core.Services;

namespace HassWebView.Core.Views;

public partial class HassAuthPage : ContentPage, IKeyHandler
{
    private enum PageState { NeedsAuth, InLoginFlow }
    private PageState _state = PageState.NeedsAuth;

    private readonly HassPageOptions _pageOptions;
    private readonly IAuthStore _authStore;
    private readonly IHassApiService _hassApiService;
    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;

    // Constructor to accept all necessary services from HassPage
    public HassAuthPage(HassPageOptions pageOptions, IHassApiService hassApiService, KeyService keyService = null, HttpServer httpServer = null)
    {
        InitializeComponent();
        
        _pageOptions = pageOptions;
        _authStore = pageOptions.AuthStore;
        _hassApiService = hassApiService;
        _httpServer = httpServer;
        _keyService = keyService;
        
        var wv = webView.WebViewControl;
        wv.Navigating += OnWebViewNavigating;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // When the page appears, always start the authentication process.
        await webView.LoadEmbeddedHtml("index.html");
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"[HassAuthPage] Navigating to: {e.Url}");

        if (_state == PageState.InLoginFlow)
        {
            var uri = new Uri(e.Url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return;

            e.Cancel = true; // Stop navigation, we have the code

            var hassUrl = await _authStore.GetHassUrlAsync();
            var hassAuth = new HassAuth(hassUrl);
            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null)
            {
                await ShowErrorAndStay("无法获取凭据，请重试。");
                return;
            }

            await _authStore.SetAccessTokenAsync(tokenResult.AccessToken);
            await _authStore.SetRefreshTokenAsync(tokenResult.RefreshToken);
            await _authStore.SetTokenExpiryUtcAsync(DateTime.UtcNow.AddSeconds(tokenResult.ExpiresIn));

            var hassApi = new HassRestApi(hassUrl, async (force) => await _authStore.GetAccessTokenAsync());
            _hassApiService.Initialize(hassApi);

            var deviceId = await _authStore.GetDeviceIdAsync();
            var pushUrl = string.IsNullOrEmpty(_pageOptions.PushUrl) && _httpServer != null ? _httpServer.BaseUrl : _pageOptions.PushUrl;
            var registrationRequest = new MobileAppRegistrationRequest
            {
                AppId = AppInfo.Current.PackageName,
                AppName = AppInfo.Current.Name,
                AppVersion = AppInfo.Current.VersionString,
                DeviceId = deviceId,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsName = DeviceInfo.Current.Platform.ToString(),
                OsVersion = DeviceInfo.Current.VersionString,
                SupportsEncryption = false,
                AppData = new MobileAppData(deviceId, pushUrl)
            };

            var registrationResult = await hassApi.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null)
            {
                await ShowErrorAndStay("注册应用失败，请检查您的Home Assistant配置。");
                return;
            }

            await _authStore.SetWebhookIdAsync(registrationResult.WebhookId);

            // Authentication successful, close the modal page.
            await MainThread.InvokeOnMainThreadAsync(() => Navigation.PopModalAsync());
        }
    }

    private async void OnExternalBusMessageReceived(object sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        var wv = webView.WebViewControl;

        var msg = JsonNode.Parse(message);
        var type = msg?["type"]?.GetValue<string>();

        switch (type)
        {
            case "webview/auth":
                var urlFromForm = msg?["data"]?.GetValue<string>();
                var auth = new HassAuth(urlFromForm);
                if (await auth.CheckApiStatusAsync())
                {
                    await _authStore.SetHassUrlAsync(auth.BaseUrl);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _state = PageState.InLoginFlow;
                        wv.Source = auth.AuthorizeUri;
                    });
                }
                else
                {
                    wv.WindowExternalBus(new { type = "webview/auth", message = "无法访问提供的URL，请确保它是正确的Home Assistant实例地址。" });
                }
                break;
            
            case "webview/config":
                 var hassUrl = await _authStore.GetHassUrlAsync();
                 wv.WindowExternalBus(new { type = "webview/config", data = new { hassUrl, remoteUrl = (string)null } });
                 break;
        }
    }

    // Display an error toast and remain on the auth page.
    private Task ShowErrorAndStay(string message)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _state = PageState.NeedsAuth;
            await webView.LoadEmbeddedHtml("index.html");
            if (!string.IsNullOrEmpty(message)) ToastService.Show(message);
        });
    }

    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    // Handle back press: go back in WebView if possible, otherwise do nothing.
    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        if (args.KeyName == "Back")
        {
            if (webView.WebViewControl.CanGoBack)
            {
                webView.WebViewControl.GoBack();
            }
            // If we can't go back, do nothing. The user can close the modal page manually.
        }
        else
        {
            webView.OnSingleClick(args.KeyName);
        }
    }

    public void OnDoubleClick(RemoteKeyEventArgs args) => webView.OnDoubleClick(args.KeyName);

    public void OnLongClick(RemoteKeyEventArgs args) => webView.OnLongClick(args.KeyName);

    #endregion
}
