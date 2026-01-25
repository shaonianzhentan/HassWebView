using HassApi;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;

namespace HassWebView.Core.Views;

public partial class HassAuthView : ContentView
{
    private readonly IHassAuthService _authService;
    private readonly KeyService _keyService;


    public event EventHandler LoggedOut;

    public enum ModeEnum
    {
        Login,
        Browser
    }

    public ModeEnum Mode {  get; set; }

    public string HassUrl { get; set; }
    public string ClientId { get; set; }
    public string DeviceId { get; set; }
    public string PushUrl { get; set; }

    public HassAuthView()
    {
        InitializeComponent();

        _authService = IPlatformApplication.Current.Services.GetRequiredService<IHassAuthService>();
        _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();

        cursor.IsVisible = _keyService != null;

        wv.Navigating += OnWebViewNavigating;
        wv.AuthTokenRequested += OnWebViewAuthTokenRequested;
        wv.LogoutRequested += OnWebViewLogoutRequested;
    }

    public void LoadLoginMode(string hassUrl, string clientId, string deviceId, string pushUrl)
    {
        Mode = ModeEnum.Login;
        HassUrl = hassUrl;
        ClientId = clientId;
        DeviceId = deviceId;
        PushUrl = pushUrl;
        HassAuth hassAuth = new HassAuth(hassUrl, clientId);
        wv.Source = hassAuth.AuthorizeUri;
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (Mode == ModeEnum.Login)
        {
            var redirectUrl = await _authService.ProcessAuthorizationCallbackAsync(new Uri(e.Url), HassUrl, ClientId, DeviceId, PushUrl);
            // 登录成功后，切换到浏览器模式
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                this.Mode = ModeEnum.Browser;
                wv.Source = redirectUrl;
            }
        }
    }

    private async void OnWebViewAuthTokenRequested(object sender, EventArgs e)
    {
        var token = await _authService.RefreshAccessTokenAsync();
        if (token != null)
        {
            var js = $"window.externalAuthSetToken(true, {{ access_token: '{token.AccessToken}', expires_in: {token.ExpiresIn} }});";
            MainThread.BeginInvokeOnMainThread(() => wv.EvaluateJavaScriptAsync(js));
        }
    }

    private async void OnWebViewLogoutRequested(object? sender, EventArgs e)
    {
        _authService.Logout();
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }



}