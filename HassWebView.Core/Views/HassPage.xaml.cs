using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
using HassWebView.Core.Auth;
using HassWebView.Core.Services;
using HassWebView.Core.Interfaces;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;

namespace HassWebView.Core.Views;

public partial class HassPage : ContentPage, IKeyHandler
{
    // Page state is now simpler: just initializing or fully authenticated.
    private enum PageState { Initializing, Authenticated }
    private PageState _state = PageState.Initializing;

    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;
    private readonly IRemoteControlService _remoteControlService;
    private readonly HassPageOptions _pageOptions;
    private readonly IAuthStore _authStore;
    private readonly IHassApiService _hassApiService;
    private DateTime? _lastBackPressTime;
    private bool _isAuthPagePresented = false; // Prevents re-entrant navigation
    private bool _authDismissed = false; // Prevents re-showing auth after user dismissed it

    public HassPage(HassPageOptions pageOptions, IHassApiService hassApiService, KeyService keyService = null, HttpServer httpServer = null, IRemoteControlService remoteControlService = null)
    {
        InitializeComponent();

        _pageOptions = pageOptions;
        _authStore = pageOptions.AuthStore;
        _keyService = keyService;
        _httpServer = httpServer;
        _hassApiService = hassApiService;
        _remoteControlService = remoteControlService;


        var wv = webView.WebViewControl;
        wv.Navigating += OnWebViewNavigating;
        wv.Navigated += Wv_Navigated;
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;

        Loaded += OnPageLoaded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _remoteControlService?.SetActiveControl(webView);

        // The core logic now resides here to be executed every time the page appears.
        await CheckAuthAndLoadAsync();
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // 校验 PushUrl，若为空则弹出警告并退出
        var pushUrl = _pageOptions.GetPushUrl?.Invoke();
        if (string.IsNullOrEmpty(pushUrl))
        {
            await DisplayAlert("配置错误", "未配置 HttpServer，PushUrl 为空，应用无法正常运行。", "退出");
            Application.Current.Quit();
            return;
        }
    }

    private async Task CheckAuthAndLoadAsync()
    {
        // If auth page is already shown, do nothing.
        if (_isAuthPagePresented) return;

        // If user explicitly dismissed auth, don't re-show it.
        if (_authDismissed) return;

        var hassUrl = await _authStore.GetHassUrlAsync();
        var refreshToken = await _authStore.GetRefreshTokenAsync();
        var webhookId = await _authStore.GetWebhookIdAsync();

        if (string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(webhookId))
        {
            await NavigateToAuthPage("请登录到您的Home Assistant实例。");
            return;
        }

        var tokenResult = await RefreshAccessTokenAsync(forceRefresh: false);
        if (tokenResult == null)
        {
            await NavigateToAuthPage("会话已过期，请重新登录。");
            return;
        }

        // If we are already authenticated and the page is loaded, don't reload.
        if (_state == PageState.Authenticated) return;

        await UpdateDeviceRegistration();

        _state = PageState.Authenticated;
        var hassAuth = new HassAuth(hassUrl);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            webView.WebViewControl.Source = new UrlWebViewSource { Url = hassAuth.RedirectUri };
        });
    }

    private async Task UpdateDeviceRegistration()
    {
        var hassUrl = await _authStore.GetHassUrlAsync();
        var webhookId = await _authStore.GetWebhookIdAsync();
        var deviceId = await _authStore.GetDeviceIdAsync();
        var pushUrl = _pageOptions.GetPushUrl();

        var mobileApp = new MobileApp(hassUrl, webhookId);
        await mobileApp.UpdateRegistrationAsync(new UpdateRegistrationRequest
        {
            AppVersion = AppInfo.Current.VersionString,
            DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
            Model = DeviceInfo.Current.Model,
            Manufacturer = DeviceInfo.Current.Manufacturer,
            OsVersion = DeviceInfo.Current.VersionString,
            AppData = new MobileAppData(deviceId, pushUrl)
        });
    }

    private void Wv_Navigated(object sender, WebNavigatedEventArgs e)
    {
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (_state == PageState.Authenticated && Uri.TryCreate(e.Url, UriKind.Absolute, out var navUri))
        {
            var hassUrl = await _authStore.GetHassUrlAsync();
            if (Uri.TryCreate(hassUrl, UriKind.Absolute, out var hassUri) && navUri.Host != hassUri.Host)
            {
                e.Cancel = true;
                _pageOptions.OpenWebPage?.Invoke(e.Url);
            }
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
            case "config/get":
                var id = msg?["id"]?.GetValue<int>();
                wv.WindowExternalBus(new { id, type = "result", success = true, result = new { hasSettingsScreen = _pageOptions.ShowSettingsScreen != null, canWriteTag = false } });
                break;
            case "config_screen/show":
                _pageOptions.ShowSettingsScreen?.Invoke();
                break;
            case "webview/config":
                var hassUrl = await _authStore.GetHassUrlAsync();
                string remoteUrl = _httpServer != null ? _httpServer.BaseUrl + "webview/remote" : null;
                wv.WindowExternalBus(new { type = "webview/config", data = new { hassUrl, remoteUrl } });
                break;
        }
    }

    protected override void OnDisappearing()
    {
        _keyService?.StopRepeatingAction();
        _remoteControlService?.ClearActiveControl(webView);
        base.OnDisappearing();
    }

    private async void OnWebViewAuthTokenRequested(object sender, EventArgs e)
    {
        var token = await RefreshAccessTokenAsync(forceRefresh: false);
        if (token != null)
        {
            var tokenExpiry = await _authStore.GetTokenExpiryUtcAsync();
            var expiresIn = (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds;
            await webView.WebViewControl.EvaluateJavaScriptAsync($"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {expiresIn} }});");
        }
        else
        {
            await webView.WebViewControl.EvaluateJavaScriptAsync("window.externalAuthSetToken(false);");
            await NavigateToAuthPage("会话已过期，请重新登录。");
        }
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        await _authStore.ClearTokensAsync();
        _state = PageState.Initializing; // Reset state
        await NavigateToAuthPage("已成功登出。");
    }

    private async Task NavigateToAuthPage(string message)
    {
        if (_isAuthPagePresented) return;
        _isAuthPagePresented = true;
        _authDismissed = false; // Reset when explicitly navigating to auth

        if (!string.IsNullOrEmpty(message)) ToastService.Show(webView, message);

        // Create the auth page, passing all necessary dependencies.
        var authPage = new HassAuthPage(_pageOptions, _hassApiService, _keyService, _httpServer);

        // When auth page is closed, check if authentication was successful.
        // If not, mark as dismissed to prevent re-showing auth in OnAppearing.
        authPage.Disappearing += (s, e) =>
        {
            if (!authPage.IsAuthenticated)
                _authDismissed = true;
        };

        await MainThread.InvokeOnMainThreadAsync(() => Navigation.PushModalAsync(authPage));
        
        _isAuthPagePresented = false; // Reset after navigation
    }

    public async Task<AuthorizationResult> RefreshAccessTokenAsync(bool forceRefresh = false)
    {
        var accessToken = await _authStore.GetAccessTokenAsync();
        var tokenExpiry = await _authStore.GetTokenExpiryUtcAsync();

        if (!forceRefresh && !string.IsNullOrEmpty(accessToken) && DateTime.UtcNow < tokenExpiry.AddSeconds(-60))
        {
            return new AuthorizationResult(accessToken, (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds, "Bearer")
            { RefreshToken = await _authStore.GetRefreshTokenAsync() };
        }

        try
        {
            var refreshToken = await _authStore.GetRefreshTokenAsync();
            var hassUrl = await _authStore.GetHassUrlAsync();
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(hassUrl)) return null;

            var hassAuth = new HassAuth(hassUrl);
            var result = await hassAuth.GetAccessTokenAsync(refreshToken);
            if (result == null) return null;

            await _authStore.SetAccessTokenAsync(result.AccessToken);
            await _authStore.SetTokenExpiryUtcAsync(DateTime.UtcNow.AddSeconds(result.ExpiresIn));
            if (!string.IsNullOrEmpty(result.RefreshToken)) await _authStore.SetRefreshTokenAsync(result.RefreshToken);

            return result;
        }
        catch (Exception ex)
        { 
            Debug.WriteLine($"[Auth] Critical error on refresh: {ex.Message}"); 
            return null; 
        }
    }

    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    public async void OnSingleClick(RemoteKeyEventArgs args)
    {
        if (args.KeyName == "Back")
        {
            if (webView.WebViewControl.CanGoBack)
            {
                _lastBackPressTime = null;
                webView.WebViewControl.GoBack();
            }
            else
            {
                if (_lastBackPressTime.HasValue && (DateTime.UtcNow - _lastBackPressTime.Value).TotalSeconds < 2)
                    Application.Current.Quit();
                else
                {
                    _lastBackPressTime = DateTime.UtcNow;
                    ToastService.Show(webView, "再按一次退出应用");
                }
            }
        }
        else
        {
            _lastBackPressTime = null;
            webView.OnSingleClick(args.KeyName);
        }
    }

    public void OnDoubleClick(RemoteKeyEventArgs args) => webView.OnDoubleClick(args.KeyName);

    public async void OnLongClick(RemoteKeyEventArgs args)
    {
        _lastBackPressTime = null;
        if (webView.OnLongClick(args.KeyName)) return;

    }

    #endregion
}
