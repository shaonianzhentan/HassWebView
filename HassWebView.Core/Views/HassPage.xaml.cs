using HassWebView.Core.Configuration;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace HassWebView.Core.Views;

[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(DeviceId), "deviceId")]
[QueryProperty(nameof(PushUrl), "pushUrl")]
public partial class HassPage : ContentPage
{
    public string Url { get; set; }
    public string Mode { get; set; }

    public string DeviceId
    {
        get
        {
            return Preferences.Get("DeviceId", "");
        }
        set
        {
            if (!string.IsNullOrEmpty(value)) Preferences.Set("DeviceId", value);
        }
    }
    public string PushUrl
    {
        get {
            return Preferences.Get("PushUrl", "");
        }
        set
        {
            if (!string.IsNullOrEmpty(value)) Preferences.Set("PushUrl", value);
        }
    }
    public string HassUrl
    {
        get
        {
            return Preferences.Get("HassUrl", "");
        }
        set
        {
            Preferences.Set("HassUrl", value);
        }
    }
    public string WebhookId
    {
        get
        {
            return Preferences.Get("WebhookId", "");
        }
        set
        {
            Preferences.Set("WebhookId", value);
        }
    }
    public string RefreshToken
    {
        get
        {
            return Preferences.Get("RefreshToken", "");
        }
        set
        {
            Preferences.Set("RefreshToken", value);
        }
    }
    public string AccessToken
    {
        get
        {
            return Preferences.Get("AccessToken", "");
        }
        set
        {
            Preferences.Set("AccessToken", value);
        }
    }
    public int ExpiresIn
    {
        get
        {
            return Preferences.Get("ExpiresIn", 0);
        }
        set
        {
            Preferences.Set("ExpiresIn", value);
        }
    }

    



    private readonly KeyService _keyService;
    private readonly HassWebViewOptions _options;
    private readonly HttpClient _httpClient = new();
    private readonly CursorControl _cursorControl;
    private bool _isInitialized = false; // The flag to ensure one-time initialization

    public HassPage()
    {
        InitializeComponent();

        _keyService = IPlatformApplication.Current.Services.GetService<KeyService>();
        _options = IPlatformApplication.Current.Services.GetRequiredService<HassWebViewOptions>();

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
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

        string effectiveMode = Mode?.ToLower();

        // These branches are for explicit navigation actions and should always run.
        if (effectiveMode == "auth")
        {
            LoadEmbeddedHtml("HassWebView.Core.Resources.index.html");
            return;
        }
        if (!string.IsNullOrEmpty(Url))
        {
            wv.Source = new UrlWebViewSource { Url = this.Url };
            return;
        }

        // This block is the initial entry point. It should only run ONCE.
        if (_isInitialized)
        {
            return; // Initialization is already complete, do nothing on subsequent visits.
        }
        _isInitialized = true; // Set the flag immediately to prevent re-entry.

        // --- Start of one-time initialization logic ---
        if (string.IsNullOrEmpty(HassUrl) || string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(WebhookId))
        {
            await GoToAuthMode();
        }
        else
        {
            // 检查是否授权
            var tokenResult = await RefreshAccessTokenAsync();
            if (tokenResult == null) {

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
                AppData = new MobileAppData(DeviceId, PushUrl)
            });


            var hassAuth = new HassAuth(HassUrl);
            var navigationUrl = $"/{nameof(HassPage)}?url={Uri.EscapeDataString(hassAuth.RedirectUri)}";
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));

        }
        // --- End of one-time initialization logic ---
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine(e.Url);
        var navigatingUrl = e.Url;
        string effectiveMode = Mode?.ToLower();

        if (effectiveMode == "login")
        {
            var uri = new Uri(navigatingUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return;

            var hassUrl = HassUrl;
            var hassAuth = new HassAuth(hassUrl);
            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null) return;

            // Store tokens and identifiers
            Preferences.Set("AccessToken", tokenResult.AccessToken);
            Preferences.Set("RefreshToken", tokenResult.RefreshToken);
            Preferences.Set("ExpiresIn", tokenResult.ExpiresIn);

            var deviceId = DeviceId;

            var hassClient = new HassClient(hassUrl, tokenResult.AccessToken);
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
                AppData = new MobileAppData(deviceId, PushUrl)
            };

            var registrationResult = await hassClient.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null) return;

            Preferences.Set("WebhookId", registrationResult.WebhookId);
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
                boxSizing: 'border-box', zIndex: '9999', overflowY: 'auto'
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
                //window.location.href = videoUrl;
                window.externalApp.externalBus(JSON.stringify({{
                    type: 'video/play',
                    data: videoUrl
                }}))
            }});
        }}

        container.insertBefore(item, container.firstChild);

        while (container.children.length > 10) {{
            container.removeChild(container.lastChild);
        }}
    }}
    
    addVideoToPanel('{urlString}');
}})();";
            
             wv.EvaluateJavaScriptAsync(script);
            });
        }
    }

    private async void OnExternalBusMessageReceived(object? sender, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        try
        {
            var msg = JsonNode.Parse(message);
            var type = msg?["type"]?.GetValue<string>();
            if (type == "config/get")
            {
                var id = msg["id"]?.GetValue<int>();

                var response = new
                {
                    id,
                    type = "result",
                    success = true,
                    result = new
                    {
                        hasSettingsScreen = true,
                        canWriteTag = false
                    }
                };

                var responseJson = JsonSerializer.Serialize(response);
                var js = $"window.externalBus({responseJson});";

                await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
            }
            else if (type == "config_screen/show")
            {
                _options.ShowSettingsScreen?.Invoke();
            }
            else if (type == "video/play")
            {
                var videoUrl = msg?["data"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(videoUrl))
                {
                    _options.PlayVideo?.Invoke(videoUrl);
                }
            }
            else if (type == "video/open")
            {
                var videoUrl = msg?["data"]?.GetValue<string>();
                
            }else if(type == "auth")
            {
                var urlFromForm = msg?["data"]?.GetValue<string>();

                bool isValid = await IsHassUrlValid(urlFromForm);
                if (isValid)
                {
                    var uri = new Uri(urlFromForm);
                    HassUrl = $"{uri.Scheme}://{uri.Authority}";
                    HassAuth hassAuth = new HassAuth(HassUrl);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        this.Mode = "login";
                        wv.Source = hassAuth.AuthorizeUri;
                    });
                }
                else
                {
                    await DisplayAlert("Validation Failed", "This does not appear to be a valid Home Assistant URL.", "OK");
                }
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
                var htmlContent = reader.ReadToEnd();
                wv.Source = new HtmlWebViewSource { Html = htmlContent };
            }
        }
    }

    private async Task<bool> IsHassUrlValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        try
        {
            var uri = new Uri(url);
            var apiUrl = new Uri(uri, "/api/").ToString();
            var response = await _httpClient.GetAsync(apiUrl);
            // A valid Home Assistant instance should return 401 Unauthorized
            // when accessing the API endpoint without credentials.
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
        var token = await RefreshAccessTokenAsync();
        if (token != null)
        {
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {token.ExpiresIn} }});";
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync("window.externalAuthSetToken(false);"));
            Logout();
        }
    }


    public void Logout()
    {
        this.WebhookId = "";
        Debug.WriteLine("Specific authentication data cleared.");
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        Logout();
        _isInitialized = false; // Reset the flag on logout
        await GoToAuthMode();
    }

    private Task GoToAuthMode()
    {
        var navigationUrl = $"/{nameof(HassPage)}?mode=auth";
        return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
    }




    public async Task<AuthorizationResult> RefreshAccessTokenAsync()
    {
        try
        {
            var hassUrl = HassUrl;
            var refreshToken = RefreshToken;

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(hassUrl))
            {
                return null;
            }

            var hassAuth = new HassAuth(hassUrl);
            var result = await hassAuth.GetAccessTokenAsync(refreshToken);
            if (result == null) return null;

            AccessToken = result.AccessToken;
            ExpiresIn = result.ExpiresIn;

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing access token: {ex.Message}");
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

    private bool OnFilterKeyDown(object sender, RemoteKeyEventArgs e)
    {
        if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown") return false;
        return true;
    }

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
                    var script = "(function() { var div = document.getElementById('video-panel-container'); if (div) { div.style.display = div.style.display === 'none' ? 'flex' : 'none'; } })();";
                    wv.EvaluateJavaScriptAsync(script);
                    break;
            }
        });
    }

    private void OnDoubleClick(object sender, RemoteKeyEventArgs e)
    {
        if (_cursorControl is null) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (e.KeyName)
            {
                case "Enter": case "DpadCenter": _cursorControl.DoubleClick(); break;
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
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var repeatInterval = 100;
            switch (e.KeyName)
            {
                case "Up": case "DpadUp": _keyService.StartRepeatingAction(() => _cursorControl?.MoveUpBy(), repeatInterval); break;
                case "Down": case "DpadDown": _keyService.StartRepeatingAction(() => _cursorControl?.MoveDownBy(), repeatInterval); break;
                case "Left": case "DpadLeft": _keyService.StartRepeatingAction(() => _cursorControl?.MoveLeftBy(), repeatInterval); break;
                case "Right": case "DpadRight": _keyService.StartRepeatingAction(() => _cursorControl?.MoveRightBy(), repeatInterval); break;
                case "Escape":
                case "Back":
                    var hassUrl = HassUrl;
                    if (!string.IsNullOrEmpty(hassUrl))
                    {
                        wv.Source = new HassAuth(hassUrl).RedirectUri;
                    }
                    break;
            }
        });
    }

    #endregion
}
