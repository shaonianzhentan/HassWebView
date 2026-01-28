using HassApi;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Reflection;

namespace HassWebView.Core.Views;

[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(ClientId), "clientId")]
[QueryProperty(nameof(DeviceId), "deviceId")]
[QueryProperty(nameof(PushUrl), "pushUrl")]
public partial class HassPage : ContentPage
{
    public string Url { get; set; }
    public string Mode { get; set; }
    public string ClientId { get; set; }
    public string DeviceId { get; set; }
    public string PushUrl { get; set; }

    private readonly IHassAuthService _authService;
    private readonly KeyService _keyService;
    private readonly HttpClient _httpClient = new();
    private readonly CursorControl _cursorControl;

    public HassPage()
    {
        InitializeComponent();

        _authService = IPlatformApplication.Current.Services.GetRequiredService<IHassAuthService>();
        _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
        }

        wv.Navigating += OnWebViewNavigating;
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
        wv.UrlSubmitted += OnUrlSubmitted;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Mode?.ToLower() == "login" && !string.IsNullOrEmpty(Url))
        {
            StartAuthentication(Url, ClientId);
        }
        else if (!string.IsNullOrEmpty(Url))
        {
            wv.Source = new UrlWebViewSource { Url = this.Url };
        }
        else
        {
            LoadUrlInputView();
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

    private async void OnUrlSubmitted(object sender, string urlFromJs)
    {
        bool isValid = await ValidateHassUrlAsync(urlFromJs);
        if (isValid)
        {
            var navigationUrl = $"//{nameof(HassPage)}?mode=login&url={Uri.EscapeDataString(urlFromJs)}" +
                                $"&clientId={Uri.EscapeDataString(this.ClientId)}" +
                                $"&deviceId={Uri.EscapeDataString(this.DeviceId)}" +
                                $"&pushUrl={Uri.EscapeDataString(this.PushUrl)}";
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
        }
        else
        {
            await DisplayAlert("验证失败", "这不是一个有效的 Home Assistant 地址，请重新输入。", "确定");
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (Mode?.ToLower() == "login")
        {
            var redirectUrl = await _authService.ProcessAuthorizationCallbackAsync(new Uri(e.Url), this.Url, this.ClientId, this.DeviceId, this.PushUrl);
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                e.Cancel = true;
                var navigationUrl = $"//{nameof(HassPage)}?url={Uri.EscapeDataString(redirectUrl)}";
                await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
            }
        }
    }

    private async void OnWebViewAuthTokenRequested(object sender, EventArgs e)
    {
        var token = await _authService.RefreshAccessTokenAsync();
        if (token != null)
        {
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {token.ExpiresIn} }});";
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
        }
    }

    private void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        _authService.Logout();
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync($"//{nameof(HassPage)}"));
    }

    private void LoadUrlInputView()
    {
        var assembly = typeof(HassPage).GetTypeInfo().Assembly;
        string resourceName = "HassWebView.Core.Resources.index.html";
        string htmlContent;

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                wv.Source = new HtmlWebViewSource { Html = "<h1>Error: Embedded resource not found.</h1>" };
                return;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                htmlContent = reader.ReadToEnd();
            }
        }
        wv.Source = new HtmlWebViewSource { Html = htmlContent };
    }

    private void StartAuthentication(string hassUrl, string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            DisplayAlert("错误", "客户端ID丢失，无法开始授权。", "确定");
            return;
        }
        HassAuth hassAuth = new HassAuth(hassUrl, clientId);
        wv.Source = new UrlWebViewSource { Url = hassAuth.AuthorizeUri.ToString() };
    }

    private async Task<bool> ValidateHassUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        try
        {
            var discoveryUrl = new Uri(new Uri(url), "/api/discovery_info").ToString();
            var response = await _httpClient.GetAsync(discoveryUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content.Contains("base_url") && content.Contains("location_name");
            }
            return false;
        }
        catch
        {
            return false;
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
        // Prevent volume keys from being processed by this page
        if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
        {
            return false;
        }
        return true;
    }

    private void OnSingleClick(object sender, RemoteKeyEventArgs e)
    {
        Debug.WriteLine($"OnSingleClick = {e.KeyName}");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (e.KeyName)
            {
                case "Enter":
                case "DpadCenter":
                    _cursorControl.Click();
                    break;
                case "Escape":
                case "Back":
                    if (wv.CanGoBack)
                    {
                        wv.GoBack();
                    }
                    break;
                case "Up":
                case "DpadUp":
                    _cursorControl.MoveUpBy();
                    break;
                case "Down":
                case "DpadDown":
                    _cursorControl.MoveDownBy();
                    break;
                case "Left":
                case "DpadLeft":
                    _cursorControl.MoveLeftBy();
                    break;
                case "Right":
                case "DpadRight":
                    _cursorControl.MoveRightBy();
                    break;
                case "Menu":
                    // VideoService.ToggleVideoPanel(wv); // Assuming VideoService exists
                    break;
            }
        });
    }

    private void OnDoubleClick(object sender, RemoteKeyEventArgs e)
    {
        Debug.WriteLine($"OnDoubleClick = {e.KeyName}");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_cursorControl is null) return;
            switch (e.KeyName)
            {
                case "Enter":
                case "DpadCenter":
                    _cursorControl.DoubleClick();
                    break;
                case "Up":
                case "DpadUp":
                    _cursorControl.SlideUp();
                    break;
                case "Down":
                case "DpadDown":
                    _cursorControl.SlideDown();
                    break;
                case "Left":
                case "DpadLeft":
                    _cursorControl.SlideLeft();
                    break;
                case "Right":
                case "DpadRight":
                    _cursorControl.SlideRight();
                    break;
            }
        });
    }

    private void OnLongClick(object sender, RemoteKeyEventArgs e)
    {
        Debug.WriteLine($"OnLongClick = {e.KeyName}");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_cursorControl is null) return;
            var repeatInterval = 100;
            switch (e.KeyName)
            {
                case "Up":
                case "DpadUp":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveUpBy(), repeatInterval);
                    break;
                case "Down":
                case "DpadDown":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveDownBy(), repeatInterval);
                    break;
                case "Left":
                case "DpadLeft":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveLeftBy(), repeatInterval);
                    break;
                case "Right":
                case "DpadRight":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveRightBy(), repeatInterval);
                    break;
                case "Escape":
                case "Back":
                    // Optimized: Use existing properties instead of re-reading from SecureStorage
                    if (!string.IsNullOrEmpty(this.Url) && !string.IsNullOrEmpty(this.ClientId))
                    {
                        HassAuth hassAuth = new HassAuth(this.Url, this.ClientId);
                        wv.Source = hassAuth.RedirectUri;
                    }
                    break;
            }
        });
    }

    #endregion
}
