using HassApi;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Web;

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
    private bool _isInitialized = false; // The flag to ensure one-time initialization

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
        if (effectiveMode == "login" && !string.IsNullOrEmpty(Url))
        {
            StartAuthentication(Url, ClientId);
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
        var webhookId = await _authService.GetWebhookIdAsync();
        if (string.IsNullOrEmpty(webhookId))
        {
            // Not logged in, navigate to auth mode.
            await GoToAuthMode();
        }
        else
        {
            // Already logged in, navigate to the HA instance.
            var hassUrl = await _authService.GetHassUrlAsync();
            var clientId = await _authService.GetClientIdAsync();
            if (!string.IsNullOrEmpty(hassUrl) && !string.IsNullOrEmpty(clientId))
            {
                var hassAuth = new HassAuth(hassUrl, clientId);
                var navigationUrl = $"//{nameof(HassPage)}?url={Uri.EscapeDataString(hassAuth.RedirectUri)}";
                await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
            }
            else
            {
                // Data is inconsistent, force re-authentication.
                await GoToAuthMode();
            }
        }
        // --- End of one-time initialization logic ---
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var navigatingUrl = e.Url;
        string effectiveMode = Mode?.ToLower();

        if (effectiveMode == "auth" && navigatingUrl.Contains("?url="))
        {
            e.Cancel = true; // Stop the navigation

            var uri = new Uri(navigatingUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            var urlFromForm = queryParams["url"];

            bool isValid = await IsHassUrlValid(urlFromForm);
            if (isValid)
            {
                var navigationUrl = $"//{nameof(HassPage)}?mode=login&url={Uri.EscapeDataString(urlFromForm)}" +
                                    $"&clientId={Uri.EscapeDataString(this.ClientId)}" +
                                    $"&deviceId={Uri.EscapeDataString(this.DeviceId)}" +
                                    $"&pushUrl={Uri.EscapeDataString(this.PushUrl)}";
                await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
            }
            else
            {
                await DisplayAlert("Validation Failed", "This does not appear to be a valid Home Assistant URL.", "OK");
            }
            return;
        }

        if (effectiveMode == "login")
        {
            var redirectUrl = await _authService.ProcessAuthorizationCallbackAsync(new Uri(navigatingUrl), this.Url, this.ClientId, this.DeviceId, this.PushUrl);
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                e.Cancel = true;
                var navigationUrl = $"//{nameof(HassPage)}?url={Uri.EscapeDataString(redirectUrl)}";
                await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
            }
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
        var token = await _authService.RefreshAccessTokenAsync();
        if (token != null)
        {
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {token.ExpiresIn} }});";
            await MainThread.InvokeOnMainThreadAsync(() => wv.EvaluateJavaScriptAsync(js));
        }
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        _authService.Logout();
        _isInitialized = false; // Reset the flag on logout
        await GoToAuthMode();
    }

    private Task GoToAuthMode()
    {
        var navigationUrl = $"//{nameof(HassPage)}?mode=auth" +
                            $"&clientId={Uri.EscapeDataString(this.ClientId)}" +
                            $"&deviceId={Uri.EscapeDataString(this.DeviceId)}" +
                            $"&pushUrl={Uri.EscapeDataString(this.PushUrl)}";
        return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(navigationUrl));
    }

    private void StartAuthentication(string hassUrl, string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            DisplayAlert("Error", "Client ID is missing.", "OK");
            return;
        }
        HassAuth hassAuth = new HassAuth(hassUrl, clientId);
        wv.Source = new UrlWebViewSource { Url = hassAuth.AuthorizeUri.ToString() };
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
                case "Menu": /* VideoService.ToggleVideoPanel(wv); */ break;
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
                    var hassUrl = await _authService.GetHassUrlAsync();
                    var clientId = await _authService.GetClientIdAsync();
                    if (!string.IsNullOrEmpty(hassUrl) && !string.IsNullOrEmpty(clientId))
                    {
                        wv.Source = new HassAuth(hassUrl, clientId).RedirectUri;
                    }
                    break;
            }
        });
    }

    #endregion
}
