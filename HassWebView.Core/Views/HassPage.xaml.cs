using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
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

[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(Mode), "mode")]
public partial class HassPage : ContentPage
{
    public string Url { get; set; }
    public string Mode { get; set; }

    #region --- Persisted Authentication Properties ---

    public string DeviceId
    {
        get
        {
            var id = Preferences.Get("DeviceId", string.Empty);
            if(string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                Preferences.Set("DeviceId", id);
            }
            return id;
        }
    }
    
    public string HassUrl
    {
        get => Preferences.Get("HassUrl", "");
        set => Preferences.Set("HassUrl", value);
    }
    public string WebhookId
    {
        get => Preferences.Get("WebhookId", "");
        set => Preferences.Set("WebhookId", value);
    }
    public string RefreshToken
    {
        get => Preferences.Get("RefreshToken", "");
        set => Preferences.Set("RefreshToken", value);
    }
    public string AccessToken
    {
        get => Preferences.Get("AccessToken", "");
        set => Preferences.Set("AccessToken", value);
    }

    /// <summary>
    /// Stores the exact UTC DateTime when the access token expires.
    /// </summary>
    public DateTime TokenExpiryUtc
    {
        get => Preferences.Get("TokenExpiryUtc", DateTime.MinValue);
        set => Preferences.Set("TokenExpiryUtc", value);
    }

    #endregion

    private readonly HttpServer _httpServer;
    private readonly KeyService _keyService;
    private readonly HassWebViewOptions _options;
    private readonly HttpClient _httpClient = new();
    private readonly CursorControl _cursorControl;
    private bool _isInitialized = false;

    public HassPage()
    {
        InitializeComponent();

        LoadEmbeddedHtml("HassWebView.Core.Resources.loading.html");

        _keyService = IPlatformApplication.Current.Services.GetService<KeyService>();
        _options = IPlatformApplication.Current.Services.GetRequiredService<HassWebViewOptions>();
        _httpServer = IPlatformApplication.Current.Services.GetService<HttpServer>();

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
                        string escapedContent = text.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
                        string jsCode = $@"
(function() {{
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
        wv.ResourceLoading += OnWebViewResourceLoading;
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
        wv.ExternalBusMessageReceived += OnExternalBusMessageReceived;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (args.NavigationType == NavigationType.Pop && args.PreviousPage is HassMediaPage) return;

        string effectiveMode = Mode?.ToLower();

        if (effectiveMode == "auth")
        {
            LoadEmbeddedHtml("HassWebView.Core.Resources.index.html");
            return;
        }
        if (effectiveMode == "login" && !string.IsNullOrEmpty(Url))
        {
            var uri = new Uri(Url);
            HassUrl = $"{uri.Scheme}://{uri.Authority}";
            wv.Source = new HassAuth(HassUrl).AuthorizeUri;
            return;
        }
        if (!string.IsNullOrEmpty(Url))
        {
            wv.Source = new UrlWebViewSource { Url = this.Url };
            return;
        }

        if (_isInitialized) return;
        _isInitialized = true;

        if (string.IsNullOrEmpty(HassUrl) || string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(WebhookId))
        {
            await GoToAuthMode();
        }
        else
        {
            var tokenResult = await RefreshAccessTokenAsync(forceRefresh: false);
            if (tokenResult == null)
            {
                await GoToAuthMode();
                return;
            }

            var mobileApp = new MobileApp(HassUrl, WebhookId);
            await mobileApp.UpdateRegistrationAsync(new UpdateRegistrationRequest
            {
                AppVersion = AppInfo.Current.VersionString,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsVersion = DeviceInfo.Current.VersionString,
                AppData = new MobileAppData(DeviceId, _options.PushUrl)
            });

            var hassAuth = new HassAuth(HassUrl);
            var navigationUrl = $"/{nameof(HassPage)}?url={Uri.EscapeDataString(hassAuth.RedirectUri)}";
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine(e.Url);
        string effectiveMode = Mode?.ToLower();

        if (effectiveMode == "login")
        {
            var uri = new Uri(e.Url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return;

            var hassAuth = new HassAuth(HassUrl);
            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null) return;
            
            AccessToken = tokenResult.AccessToken;
            RefreshToken = tokenResult.RefreshToken;
            TokenExpiryUtc = DateTime.UtcNow.AddSeconds(tokenResult.ExpiresIn);

            var hassApi = new HassRestApi(HassUrl, async (force) => {
                var token = await RefreshAccessTokenAsync(force);
                return token?.AccessToken;
            });
            var registrationRequest = new MobileAppRegistrationRequest
            {
                AppId = AppInfo.Current.PackageName,
                AppName = AppInfo.Current.Name,
                AppVersion = AppInfo.Current.VersionString,
                DeviceId = DeviceId,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsName = DeviceInfo.Current.Platform.ToString(),
                OsVersion = DeviceInfo.Current.VersionString,
                SupportsEncryption = false,
                AppData = new MobileAppData(DeviceId, _options.PushUrl)
            };

            var registrationResult = await hassApi.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null) return;

            WebhookId = registrationResult.WebhookId;
            var navigationUrl = $"/{nameof(HassPage)}?url={Uri.EscapeDataString(hassAuth.RedirectUri)}";
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
        }
    }

    private void OnWebViewResourceLoading(object? sender, ResourceLoadingEventArgs e)
    {
        var urlString = e.Url.ToString();
        Debug.WriteLine($"ResourceLoading：{urlString}");
        if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) ||
             urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                 var script = @$"
(function() {{
    function addVideoToPanel(videoUrl) {{
        const safeId = 'video-panel-item-' + encodeURIComponent(videoUrl).replace(/[^a-zA-Z0-9_-]/g, '_');
        let container = document.getElementById('video-panel-container');
        if (!container) {{
            container = document.createElement('div');
            container.id = 'video-panel-container';
            Object.assign(container.style, {{
                position: 'fixed', left: 0, top: 0, height: '100%', width: '30%', minWidth: '200px',
                display: 'flex', flexDirection: 'column', gap: '8px', padding: '10px',
                backgroundColor: 'rgba(0,0,0,0.6)', borderRadius: '0 10px 10px 0',
                boxSizing: 'border-box', zIndex: '2147483647', overflowY: 'auto'
            }});
            document.body.appendChild(container);
        }}
        let item = document.getElementById(safeId);
        if (!item) {{
            item = document.createElement('div');
            item.id = safeId;
            item.textContent = videoUrl;
            Object.assign(item.style, {{
                padding: '8px', backgroundColor: 'rgba(255, 255, 255, 0.1)',
                color: '#ffffff', border: '1px solid #555', borderRadius: '5px',
                wordBreak: 'break-all', cursor: 'pointer'
            }});
            item.addEventListener('click', () => {{
                window.externalApp.externalBus(JSON.stringify({{ type: 'video/play', data: videoUrl }}));
            }});
        }}
        container.insertBefore(item, container.firstChild);
        while (container.children.length > 6) {{
            container.removeChild(container.lastChild);
        }}
    }}
    addVideoToPanel('{urlString}');
}})();";
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
                    var id = msg["id"]?.GetValue<int>();
                    wv.WindowExternalBusAsync(new { id, type = "result", success = true, result = new { hasSettingsScreen = true, canWriteTag = false } });
                    break;
                case "config_screen/show":
                    _options.ShowSettingsScreen?.Invoke();
                    break;
                case "video/play":
                    var videoUrl = msg?["data"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(videoUrl)) _options.PlayVideo?.Invoke(videoUrl);
                    break;
                case "webview/auth":
                    var urlFromForm = msg?["data"]?.GetValue<string>();
                    Debug.WriteLine($"[ExternalBus] Received auth URL: {urlFromForm}");
                    if (await IsHassUrlValid(urlFromForm))
                    {
                        var navigationUrl = $"/{nameof(HassPage)}?mode=login&url={Uri.EscapeDataString(urlFromForm)}";
                        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
                    }
                    else
                    {
                        wv.WindowExternalBusAsync(new { type = "webview/auth", message = "无法访问提供的URL，请确保它是正确的Home Assistant实例地址，并且设备能够访问它。" });
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

    private void LoadEmbeddedHtml(string resourceName)
    {
        var assembly = GetType().GetTypeInfo().Assembly;
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                wv.Source = new HtmlWebViewSource { Html = "<h1>Error: Embedded resource not found.</h1>" };
                return;
            }
            using (var reader = new StreamReader(stream))
            {
                wv.Source = new HtmlWebViewSource { Html = reader.ReadToEnd() };
            }
        }
    }

    private async Task<bool> IsHassUrlValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        try
        {
            var apiUrl = new Uri(new Uri(url), "/api/").ToString();
            var response = await _httpClient.GetAsync(apiUrl);
            return response.StatusCode == HttpStatusCode.Unauthorized;
        }
        catch
        { 
            return false;
        }
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
            var expiresIn = (int)(TokenExpiryUtc - DateTime.UtcNow).TotalSeconds;
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {expiresIn} }});";
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync("window.externalAuthSetToken(false);"));
            await GoToAuthMode();
        }
    }

    public void Logout()
    {
        AccessToken = "";
        RefreshToken = "";
        WebhookId = "";
        TokenExpiryUtc = DateTime.MinValue;
        Debug.WriteLine("[Auth] All authentication data has been cleared.");
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        Logout();
        _isInitialized = false;
        await GoToAuthMode();
    }

    private Task GoToAuthMode()
    {
        _isInitialized = false;
        var navigationUrl = $"/{nameof(HassPage)}?mode=auth";
        return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
    }

    public async Task<AuthorizationResult> RefreshAccessTokenAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && !string.IsNullOrEmpty(AccessToken) && TokenExpiryUtc != DateTime.MinValue && DateTime.UtcNow < TokenExpiryUtc.AddSeconds(-60))
        {
            Debug.WriteLine("[Auth] Using cached access token.");
            return new AuthorizationResult(this.AccessToken, (int)(this.TokenExpiryUtc - DateTime.UtcNow).TotalSeconds, "Bearer")
            {
                RefreshToken = this.RefreshToken
            };
        }

        Debug.WriteLine(forceRefresh ? "[Auth] Forcing token refresh." : "[Auth] Token expired/invalid, refreshing.");
        try
        {
            if (string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(HassUrl))
            {
                Debug.WriteLine("[Auth] Refresh failed: Missing RefreshToken or HassUrl.");
                return null;
            }

            var hassAuth = new HassAuth(HassUrl);
            var result = await hassAuth.GetAccessTokenAsync(RefreshToken);
            if (result == null)
            {
                Debug.WriteLine("[Auth] Refresh failed: GetAccessTokenAsync returned null.");
                Logout();
                await GoToAuthMode();
                return null;
            }
            
            AccessToken = result.AccessToken;
            TokenExpiryUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                RefreshToken = result.RefreshToken;
            }

            Debug.WriteLine("[Auth] Token refreshed successfully.");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Auth] Critical error on refresh: {ex.Message}");
            Logout();
            await GoToAuthMode();
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

    private void OnLongClick(object sender, RemoteKeyEventArgs e)
    {
        if (_keyService is null) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var repeatInterval = 100;
            switch (e.KeyName)
            {
                case "Up": case "DpadUp": _keyService.StartRepeatingAction(() => _cursorControl?.MoveUpBy(), repeatInterval); break;
                case "Down": case "DpadDown": _keyService.StartRepeatingAction(() => _cursorControl?.MoveDownBy(), repeatInterval); break;
                case "Left": case "DpadLeft": _keyService.StartRepeatingAction(() => _cursorControl?.MoveLeftBy(), repeatInterval); break;
                case "Right": case "DpadRight": _keyService.StartRepeatingAction(() => _cursorControl?.MoveRightBy(), repeatInterval); break;
                case "Escape": case "Back":
                    if (!string.IsNullOrEmpty(HassUrl)) wv.Source = new HassAuth(HassUrl).RedirectUri;
                    break;
            }
        });
    }

    #endregion
}
