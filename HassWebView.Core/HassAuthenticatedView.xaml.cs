using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core;

public partial class HassAuthenticatedView : ContentView
{
    private readonly IHassAuthService _authService;
    private string _lockedHost = null;

    public event EventHandler LoggedOut;

    public HassWebView WebView => webView;

    #region SourceUrl BindableProperty
    public static readonly BindableProperty SourceUrlProperty =
        BindableProperty.Create(nameof(SourceUrl), typeof(string), typeof(HassAuthenticatedView), propertyChanged: OnSourceUrlChanged);

    public string SourceUrl
    {
        get => (string)GetValue(SourceUrlProperty);
        set => SetValue(SourceUrlProperty, value);
    }

    private static void OnSourceUrlChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (HassAuthenticatedView)bindable;
        var newUrl = (string)newValue;

        if (!string.IsNullOrEmpty(newUrl) && Uri.TryCreate(newUrl, UriKind.Absolute, out var uri))
        {
            // Lock the host to prevent navigation to external sites.
            // Only set the lock on the first valid, non-authenticated URL.
            if (control._lockedHost == null && uri.Scheme.StartsWith("http"))
            {
                control._lockedHost = uri.Host;
                Debug.WriteLine($"[HassAuthenticatedView] Host is now locked to: {control._lockedHost}");
            }
            
            Debug.WriteLine($"[HassAuthenticatedView] SourceUrl set. Navigating to: {newUrl}");
            control.webView.Source = new UrlWebViewSource { Url = newUrl };
        }
    }
    #endregion

    public HassAuthenticatedView()
    {
        InitializeComponent();
        _authService = IPlatformApplication.Current.Services.GetRequiredService<IHassAuthService>();

        webView.Navigating += OnWebViewNavigating;
        webView.AuthTokenRequested += OnWebViewAuthTokenRequested;
        webView.LogoutRequested += OnWebViewLogoutRequested;
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url == null) return;

        // SECURITY: Block navigation to external domains.
        if (_lockedHost != null && e.Url.Scheme.StartsWith("http"))
        {
            if (!string.Equals(new Uri(e.Url).Host, _lockedHost, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"[HassAuthenticatedView] BLOCKED navigation to external host: {e.Url}");
                e.Cancel = true;
                return;
            }
        }
        
        // AUTH: Handle the authentication callback.
        if (e.Url.StartsWith(_authService.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true; 
            Debug.WriteLine($"[HassAuthenticatedView] Intercepted auth callback: {e.Url}");
            
            var isAuthenticated = await _authService.ProcessAuthorizationCallbackAsync(e.Url);

            if (isAuthenticated)
            {
                Debug.WriteLine("[HassAuthenticatedView] Auth callback successful. Navigating to instance URL.");
                _lockedHost = new Uri(_authService.HassUrl).Host;
                webView.Source = new UrlWebViewSource { Url = _authService.HassUrl };
            }
            else
            {
                Debug.WriteLine("[HassAuthenticatedView] Auth callback failed.");
                LoggedOut?.Invoke(this, EventArgs.Empty);
            }
            return;
        }
    }

    private async void OnWebViewAuthTokenRequested(object? sender, EventArgs e)
    {
        Debug.WriteLine("[HassAuthenticatedView] WebView is requesting a new token.");
        var token = await _authService.RefreshAccessTokenAsync();
        if (token != null && sender is HassWebView wv)
        {
            wv.SendAuthTokenToWebView(token.AccessToken, token.ExpiresIn);
        }
        else
        {
            Debug.WriteLine("[HassAuthenticatedView] Failed to refresh token. Session is now invalid.");
            webView.Source = "about:blank";
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        Debug.WriteLine("[HassAuthenticatedView] WebView requested logout.");
        await _authService.LogoutAsync();
        
        _lockedHost = null; // Clear the host lock on logout.
        webView.Source = "about:blank";
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }
}
