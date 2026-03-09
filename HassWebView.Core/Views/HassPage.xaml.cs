using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
using HassWebView.Core.Auth;
using HassWebView.Core.Services;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace HassWebView.Core.Views;

public partial class HassPage : ContentPage
{
    private enum PageState { Initializing, NeedsAuth, InLoginFlow, Authenticated }
    private PageState _state = PageState.Initializing;

    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;
    private readonly HassPageOptions _pageOptions;
    private readonly CursorControl _cursorControl;
    private readonly IAuthStore _authStore;
    private string _defaultUserAgent;

    public HassPage(HassPageOptions pageOptions, KeyService keyService = null, HttpServer httpServer = null)
    {
        InitializeComponent();

        LoadEmbeddedHtml("HassWebView.Core.Resources.loading.html");

        _pageOptions = pageOptions;
        _authStore = pageOptions.AuthStore;
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
                var assembly = GetType().GetTypeInfo().Assembly;
                using (var stream = assembly.GetManifestResourceStream("HassWebView.Core.Resources.remote.html"))
                {
                    if (stream == null)
                    {
                        await res.Html("<h1>没找到远程控制资源页面</h1>");
                        return;
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        var htmlContent = await reader.ReadToEndAsync();
                        await res.Html(htmlContent);
                    }
                }
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
                        var appendText = query["append"] == "1" ? "el.value + " : "";
                        var text = query["text"];
                        string escapedContent = text.Replace("\", "\\").Replace("'", "\'");
                        string jsCode = $@"(function() {{
    const el = document.activeElement;
    if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return;
    el.value = {appendText}'{escapedContent}';
    ['input', 'change', 'compositionstart', 'compositionend', 'blur', 'focus'].forEach(evt => {{
        el.dispatchEvent(new Event(evt, {{ bubbles: true, cancelable: true, view: window }}));
    }});
    el.selectionStart = el.selectionEnd = el.value.length;
}})();";
                        MainThread.BeginInvokeOnMainThread(() => wv.EvaluateJavaScriptAsync(jsCode));
                        break;
                }
                await res.Text("");
            });
        }

        wv.Navigating += OnWebViewNavigating;
        wv.Navigated += OnWebViewNavigated;
        wv.ResourceLoading += OnWebViewResourceLoading;
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_state != PageState.Initializing) return;

        var hassUrl = await _authStore.GetHassUrlAsync();
        var refreshToken = await _authStore.GetRefreshTokenAsync();
        var webhookId = await _authStore.GetWebhookIdAsync();

        if (string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(webhookId))
        {
            _state = PageState.NeedsAuth;
            LoadEmbeddedHtml("HassWebView.Core.Resources.index.html");
        }
        else
        {
            var tokenResult = await RefreshAccessTokenAsync(forceRefresh: false);
            if (tokenResult == null)
            {
                _state = PageState.NeedsAuth;
                LoadEmbeddedHtml("HassWebView.Core.Resources.index.html");
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
            wv.Source = new UrlWebViewSource { Url = hassAuth.RedirectUri };
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"[HassPage] Navigating to: {e.Url}");

        // Store the default UserAgent on the first navigation
        if (string.IsNullOrEmpty(_defaultUserAgent) && !string.IsNullOrEmpty(wv.UserAgent))
        {
            _defaultUserAgent = wv.UserAgent;
            Debug.WriteLine($"[HassPage] Default User-Agent captured: {_defaultUserAgent}");
        }

        // Apply domain-specific User-Agent
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
                Debug.WriteLine($"[HassPage] Applying User-Agent for {host}: {targetUserAgent}");
                wv.UserAgent = targetUserAgent;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassPage] Error applying User-Agent: {ex.Message}");
        }

        // --- YOUR ORIGINAL CODE ---
        if (_state == PageState.InLoginFlow)
        {
            var uri = new Uri(e.Url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return;

            e.Cancel = true;

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

            var hassApi = new HassRestApi(hassUrl, async (force) => {
                var token = await RefreshAccessTokenAsync(force);
                return token?.AccessToken;
            });
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

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success || e.Source is not UrlWebViewSource urlSource)
        {
            return;
        }

        Debug.WriteLine($"[HassPage] Navigated to: {urlSource.Url}");

        try
        {
            var uri = new Uri(urlSource.Url);
            var host = uri.Host;

            var config = _pageOptions.DomainConfigs?
                .FirstOrDefault(kvp => host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                .Value;

            if (config == null) return;

            // Inject CSS if it exists
            if (!string.IsNullOrWhiteSpace(config.Css))
            {
                string escapedCss = config.Css.Replace("\", "\\").Replace("`", "\`").Replace("$", "\$");
                string cssScript = $@"(function() {{
    var style = document.createElement('style');
    style.type = 'text/css';
    style.innerHTML = `{escapedCss}`;
    document.head.appendChild(style);
    console.log('[HassWebView] Injected custom CSS for {host}.');
}})();";
                await wv.EvaluateJavaScriptAsync(cssScript);
            }

            // Execute JS if it exists
            if (!string.IsNullOrWhiteSpace(config.Js))
            {
                await wv.EvaluateJavaScriptAsync(config.Js);
                Debug.WriteLine($"[HassWebView] Executed custom JS for {host}.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassPage] Error applying domain config (CSS/JS): {ex.Message}");
        }
    }

    private void OnWebViewResourceLoading(object sender, ResourceLoadingEventArgs e)
    {
        var urlString = e.Url.ToString();
        Debug.WriteLine($"ResourceLoading：{urlString}");
        if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) ||
             urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                 var script = @"(function() { function addVideoToPanel(videoUrl) { const safeId = 'video-panel-item-' + encodeURIComponent(videoUrl).replace(/[^a-zA-Z0-9_-]/g, '_'); let container = document.getElementById('video-panel-container'); if (!container) { container = document.createElement('div'); container.id = 'video-panel-container'; Object.assign(container.style, { position: 'fixed', left: 0, top: 0, height: '100%', width: '30%', minWidth: '200px', display: 'flex', flexDirection: 'column', gap: '8px', padding: '10px', backgroundColor: 'rgba(0,0,0,0.6)', borderRadius: '0 10px 10px 0', boxSizing: 'border-box', zIndex: '2147483647', overflowY: 'auto' }); document.body.appendChild(container); } let item = document.getElementById(safeId); if (!item) { item = document.createElement('div'); item.id = safeId; item.textContent = videoUrl; Object.assign(item.style, { padding: '8px', backgroundColor: 'rgba(255, 255, 255, 0.1)', color: '#ffffff', border: '1px solid #555', borderRadius: '5px', wordBreak: 'break-all', cursor: 'pointer' }); item.addEventListener('click', () => { window.externalApp.externalBus(JSON.stringify({ type: 'video/play', data: videoUrl, origin: top.location.origin })); }); } container.insertBefore(item, container.firstChild); while (container.children.length > 6) { container.removeChild(container.lastChild); } } addVideoToPanel('{urlString}'); })();";
                wv.EvaluateJavaScriptAsync(script);
            });
        }
    }

    private async void OnExternalBusMessageReceived(object sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        try
        {
            var msg = JsonNode.Parse(message);
            var type = msg?["type"]?.GetValue<string>();
            switch (type)
            {
                case "config/get":
                    var id = msg?["id"]?.GetValue<int>();
                    wv.WindowExternalBusAsync(new { id, type = "result", success = true, result = new { hasSettingsScreen = true, canWriteTag = false } });
                    break;
                case "config_screen/show":
                    _pageOptions.ShowSettingsScreen?.Invoke();
                    break;
                case "video/play":
                    var videoUrl = msg?["data"]?.GetValue<string>();
                    var origin = msg?["origin"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(videoUrl)) await DisplayVideoPlayer(videoUrl, origin);
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
                        await wv.WindowExternalBusAsync(new { type = "webview/auth", message = "无法访问提供的URL，请确保它是正确的Home Assistant实例地址，并且设备能够访问它。" });
                    }
                    break;
                case "webview/url":
                    if (_httpServer != null)
                    {
                        wv.WindowExternalBusAsync(new { type = "webview/url", data = _httpServer.BaseUrl + "webview/remote" });
                    }
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
                            wv.WindowExternalBusAsync(new { type = "x5/download", data = progress });
                        });
                        if (result) wv.WindowExternalBusAsync(new { type = "x5/init" });
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
            var mediaPage = new HassMediaPage { Url = url, BaseUrl = baseUrl };
            return Shell.Current.Navigation.PushModalAsync(mediaPage, true);
        });
    }

    private void LoadEmbeddedHtml(string resourceName)
    {
        var assembly = GetType().GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            wv.Source = new HtmlWebViewSource { Html = "<h1>Error: Embedded resource not found.</h1>" };
            return;
        }
        using var reader = new StreamReader(stream);
        wv.Source = new HtmlWebViewSource { Html = reader.ReadToEnd() };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetupKeyServiceListeners(true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SetupKeyServiceListeners(false);
    }

    private async void OnWebViewAuthTokenRequested(object sender, EventArgs e)
    {
        var token = await RefreshAccessTokenAsync(forceRefresh: false);
        if (token != null)
        {
            var tokenExpiry = await _authStore.GetTokenExpiryUtcAsync();
            var expiresIn = (int)(tokenExpiry - DateTime.UtcNow).TotalSeconds;
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {expiresIn} }});";
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync("window.externalAuthSetToken(false);"));
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
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            _state = PageState.NeedsAuth;
            LoadEmbeddedHtml("HassWebView.Core.Resources.index.html");
            if (!string.IsNullOrEmpty(message))
            {
                wv.WindowExternalBusAsync(new { type = "webview/auth", message });
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

    #region KeyService Handlers

    private void SetupKeyServiceListeners(bool subscribe)
    {
        if (_keyService is null) return;
        if (subscribe)
        {
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
            _keyService.KeyDown += OnFilterKeyDown;
        }
        else
        {
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            _keyService.KeyDown -= OnFilterKeyDown;
        }
    }

    private bool OnFilterKeyDown(object sender, RemoteKeyEventArgs e) => e.KeyName != "VolumeUp" && e.KeyName != "VolumeDown";

    private void OnSingleClick(object sender, RemoteKeyEventArgs e)
    {
        if (_cursorControl is null) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (e.KeyName)
            {
                case "Enter": case "DpadCenter": _cursorControl.Click(); break;
                case "Escape": case "Back": if (wv.CanGoBack) wv.GoBack(); break;
                case "Up": case "DpadUp": _cursorControl.MoveUpBy(); break;
                case "Down": case "DpadDown": _cursorControl.MoveDownBy(); break;
                case "Left": case "DpadLeft": _cursorControl.MoveLeftBy(); break;
                case "Right": case "DpadRight": _cursorControl.MoveRightBy(); break;
                case "Menu":
                    wv.EvaluateJavaScriptAsync("(function() { var div = document.getElementById('video-panel-container'); if (div) { div.style.display = div.style.display === 'none' ? 'flex' : 'none'; } })();");
                    break;
            }
        });
    }

    private void OnDoubleClick(object sender, RemoteKeyEventArgs e)
    {
        if (_cursorControl is null) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            switch (e.KeyName)
            {
                case "Enter": case "DpadCenter": await _cursorControl.DoubleClick(); break;
                case "Up": case "DpadUp": _cursorControl.SlideUp(); break;
                case "Down": case "DpadDown": _cursorControl.SlideDown(); break;
                case "Left": case "DpadLeft": _cursorControl.SlideLeft(); break;
                case "Right": case "DpadRight": _cursorControl.SlideRight(); break;
            }
        });
    }

    private async void OnLongClick(object sender, RemoteKeyEventArgs e)
    {
        if (_keyService is null) return;
        var hassUrl = await _authStore.GetHassUrlAsync();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var repeatInterval = 100;
            switch (e.KeyName)
            {
                case "Up": case "DpadUp": _keyService.StartRepeatingAction(() => _cursorControl?.MoveUpBy(), repeatInterval); break;
                case "Down": case "DpadDown": _keyService.StartRepeatingAction(() => _cursorControl?.MoveDownBy(), repeatInterval); break;
                case "Left": case "DpadLeft": _keyService.StartRepeatingAction(() => _cursorControl?.MoveLeftBy(), repeatInterval); break;
                case "Right": case "DpadRight": _keyService.StartRepeatingAction(() => _cursorControl.MoveRightBy(), repeatInterval); break;
                case "Escape": case "Back":
                    if (!string.IsNullOrEmpty(hassUrl)) wv.Source = new HassAuth(hassUrl).RedirectUri;
                    break;
            }
        });
    }

    #endregion
}
