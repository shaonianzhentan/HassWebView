using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
using HassWebView.Core.Auth;
using HassWebView.Core.Services;
using HassWebView.Core.Interfaces;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace HassWebView.Core.Views;

public partial class HassPage : ContentPage, IKeyHandler
{
    private enum PageState { Initializing, NeedsAuth, InLoginFlow, Authenticated }
    private PageState _state = PageState.Initializing;

    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;
    private readonly HassPageOptions _pageOptions;
    private readonly CursorControl _cursorControl;
    private readonly IAuthStore _authStore;
    private readonly IHassApiService _hassApiService;

    public HassPage(HassPageOptions pageOptions, IHassApiService hassApiService, KeyService keyService = null, HttpServer httpServer = null)
    {
        InitializeComponent();

        var wv = webView.WebView;
        var cursor = webView.Cursor;
        var root = webView.Root;

        _ = LoadEmbeddedHtml("loading.html");

        _pageOptions = pageOptions;
        _authStore = pageOptions.AuthStore;
        _keyService = keyService;
        _httpServer = httpServer;
        _hassApiService = hassApiService;

        _pageOptions.SetWebViewSource = (newSource) => MainThread.BeginInvokeOnMainThread(() => wv.Source = newSource);

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
        }

        if (_httpServer != null)
        {
            if (string.IsNullOrEmpty(_pageOptions.PushUrl))
            {
                _pageOptions.PushUrl = _httpServer.BaseUrl;
            }
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
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var wv = webView.WebView;

        if (_state != PageState.Initializing) return;

        var hassUrl = await _authStore.GetHassUrlAsync();
        var refreshToken = await _authStore.GetRefreshTokenAsync();
        var webhookId = await _authStore.GetWebhookIdAsync();

        if (string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(webhookId))
        {
            _state = PageState.NeedsAuth;
            await LoadEmbeddedHtml("index.html");
        }
        else
        {
            var tokenResult = await RefreshAccessTokenAsync(forceRefresh: false);
            if (tokenResult == null)
            {
                _state = PageState.NeedsAuth;
                await LoadEmbeddedHtml("index.html");
                return;
            }

            var deviceId = await _authStore.GetDeviceIdAsync();
            var mobileApp = new MobileApp(hassUrl, webhookId);
            await mobileApp.UpdateRegistrationAsync(new UpdateRegistrationRequest
            {
                AppVersion = AppInfo.Current.VersionString,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsVersion = DeviceInfo.Current.VersionString,
                AppData = new MobileAppData(deviceId, _pageOptions.PushUrl)
            });

            _state = PageState.Authenticated;
            var hassAuth = new HassAuth(hassUrl);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                wv.Source = new UrlWebViewSource { Url = hassAuth.RedirectUri };
            });
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"[HassPage] Navigating to: {e.Url}");

        var wv = webView.WebView;

        if (_state == PageState.Authenticated && e.NavigationEvent == WebNavigationEvent.Back && e.Url.StartsWith("about:"))
        {
            Debug.WriteLine("[HassPage] Back navigation to initial page blocked.");
            e.Cancel = true;
            return;
        }

        if (_state == PageState.InLoginFlow)
        {
            var uri = new Uri(e.Url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return;

            var hassUrl = await _authStore.GetHassUrlAsync();
            var hassAuth = new HassAuth(hassUrl);
            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null)
            {
                await GoToAuthModeWithError("无法获取凭据，请重试。");
                return;
            }

            await _authStore.SetAccessTokenAsync(tokenResult.AccessToken);
            await _authStore.SetRefreshTokenAsync(tokenResult.RefreshToken);
            await _authStore.SetTokenExpiryUtcAsync(DateTime.UtcNow.AddSeconds(tokenResult.ExpiresIn));

            var hassApi = new HassRestApi(hassUrl, async (force) =>
            {
                var token = await RefreshAccessTokenAsync(force);
                return token?.AccessToken;
            });

            _hassApiService.Initialize(hassApi);

            var deviceId = await _authStore.GetDeviceIdAsync();
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
                AppData = new MobileAppData(deviceId, _pageOptions.PushUrl)
            };

            var registrationResult = await hassApi.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null)
            {
                await GoToAuthModeWithError("注册应用失败，请检查您的Home Assistant配置。");
                return;
            }

            await _authStore.SetWebhookIdAsync(registrationResult.WebhookId);
            _state = PageState.Authenticated;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                wv.Source = new UrlWebViewSource { Url = hassAuth.RedirectUri };
            });
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
                case "config/get":
                    var id = msg?["id"]?.GetValue<int>();
                    wv.WindowExternalBus(new { id, type = "result", success = true, result = new { hasSettingsScreen = true, canWriteTag = false } });
                    break;
                case "config_screen/show":
                    _pageOptions.ShowSettingsScreen?.Invoke();
                    break;
                case "webview/auth":
                    var urlFromForm = msg?["data"]?.GetValue<string>();
                    Debug.WriteLine($"[ExternalBus] Received auth URL: {urlFromForm}");
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
                        wv.WindowExternalBus(new { type = "webview/auth", message = "无法访问提供的URL，请确保它是正确的Home Assistant实例地址，并且设备能够访问它。" });
                    }
                    break;
                case "webview/config":
                    var hassUrl = await _authStore.GetHassUrlAsync();
                    string remoteUrl = null;
                    if (_httpServer != null)
                    {
                        remoteUrl = _httpServer.BaseUrl + "webview/remote";
                    }
                    wv.WindowExternalBus(new { type = "webview/config", data = new { hassUrl, remoteUrl } });
                    break;
            }
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[ExternalBus] Error parsing JSON: {ex.Message}");
        }
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
            Debug.WriteLine($"[HassPage] Error loading embedded HTML: {ex.Message}");
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

    private async void OnWebViewAuthTokenRequested(object sender, EventArgs e)
    {
        var wv = webView.WebView;
        var token = await RefreshAccessTokenAsync(forceRefresh: false);
        if (token != null)
        {
            Debug.WriteLine("授权请求");
            var tokenExpiry = await _authStore.GetTokenExpiryUtcAsync();
            var expiresIn = (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds;
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {expiresIn} }});";
            await wv.EvaluateJavaScriptAsync(js);
        }
        else
        {
            Debug.WriteLine("会话已过期");
            await wv.EvaluateJavaScriptAsync("window.externalAuthSetToken(false);");
            await GoToAuthModeWithError("会话已过期，请重新登录。");
        }
    }

    public async Task LogoutAsync()
    {
        await _authStore.ClearTokensAsync();
        Debug.WriteLine("[Auth] All authentication data has been cleared.");
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        await LogoutAsync();
        await GoToAuthModeWithError("已成功登出。");
    }

    private Task GoToAuthModeWithError(string message)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _state = PageState.NeedsAuth;
            await LoadEmbeddedHtml("index.html");
            if (!string.IsNullOrEmpty(message))
            {
                ToastService.Show(message);
            }
        });
    }

    public async Task<AuthorizationResult> RefreshAccessTokenAsync(bool forceRefresh = false)
    {
        var accessToken = await _authStore.GetAccessTokenAsync();
        var tokenExpiry = await _authStore.GetTokenExpiryUtcAsync();

        if (!forceRefresh && !string.IsNullOrEmpty(accessToken) && tokenExpiry != DateTime.MinValue && DateTime.UtcNow < tokenExpiry.AddSeconds(-60))
        {
            Debug.WriteLine("[Auth] Using cached access token.");
            var refreshToken = await _authStore.GetRefreshTokenAsync();
            return new AuthorizationResult(accessToken, (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds, "Bearer")
            {
                RefreshToken = refreshToken
            };
        }

        Debug.WriteLine(forceRefresh ? "[Auth] Forcing token refresh." : "[Auth] Token expired/invalid, refreshing.");
        try
        {
            var refreshToken = await _authStore.GetRefreshTokenAsync();
            var hassUrl = await _authStore.GetHassUrlAsync();

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(hassUrl))
            {
                Debug.WriteLine("[Auth] Refresh failed: Missing RefreshToken or HassUrl.");
                await LogoutAsync();
                return null;
            }

            var hassAuth = new HassAuth(hassUrl);
            var result = await hassAuth.GetAccessTokenAsync(refreshToken);
            if (result == null)
            {
                Debug.WriteLine("[Auth] Refresh failed: GetAccessTokenAsync returned null.");
                await LogoutAsync();
                return null;
            }

            await _authStore.SetAccessTokenAsync(result.AccessToken);
            await _authStore.SetTokenExpiryUtcAsync(DateTime.UtcNow.AddSeconds(result.ExpiresIn));
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                await _authStore.SetRefreshTokenAsync(result.RefreshToken);
            }

            Debug.WriteLine("[Auth] Token refreshed successfully.");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Auth] Critical error on refresh: {ex.Message}");
            await LogoutAsync();
            return null;
        }
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

            var hassUrl = await _authStore.GetHassUrlAsync();
            if (args.KeyName == "Back")
            {

                if (!string.IsNullOrEmpty(hassUrl)) wv.Source = new HassAuth(hassUrl).RedirectUri;
            }
        }

    }

    #endregion

}
