using HassWebView.Core.Auth;
using HassWebView.Core.Configuration;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;
using HassWebView.HassApi;
using HassWebView.HassApi.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Runtime.InteropServices;
using HassWebView.Core.Events;

namespace HassWebView.Core.Views;

public partial class HassAuthPage : ContentPage, IKeyHandler
{
    private enum PageState { NeedsAuth, InLoginFlow }
    private PageState _state = PageState.NeedsAuth;

    /// <summary>
    /// 标记授权是否已成功完成，供外部判断 Modal 关闭原因。
    /// </summary>
    public bool IsAuthenticated { get; private set; } = false;

    private readonly HassPageOptions _pageOptions;
    private readonly IAuthStore? _authStore;
    private readonly IHassApiService _hassApiService;
    private readonly HttpServer? _httpServer;
    private readonly KeyService? _keyService;
    
    // 缓存的 Hass 实例列表（非阻塞扫描）
    private List<object>? _cachedHassInstances;
    private bool _isDiscovering;

    // Constructor to accept all necessary services from HassPage
    public HassAuthPage(HassPageOptions pageOptions, IHassApiService hassApiService, KeyService? keyService = null, HttpServer? httpServer = null)
    {
        InitializeComponent();
        
        _pageOptions = pageOptions;
        _authStore = pageOptions.AuthStore;
        _hassApiService = hassApiService;
        _httpServer = httpServer;
        _keyService = keyService;
        
        var wv = webView.WebViewControl;
        wv.Navigating += OnWebViewNavigating;
        
        RegisterHttpRoutes();
        
        // 后台启动局域网扫描（非阻塞）
        StartBackgroundDiscovery();
    }
    
    private void UnregisterHttpRoutes()
    {
        if (_httpServer == null) return;
        
        _httpServer.RemoveGet("/api/webview/config");
        _httpServer.RemoveGet("/api/hass/discover");
        _httpServer.RemovePost("/api/webview/auth");
    }
    
    private void RegisterHttpRoutes()
    {
        Debug.WriteLine($"[HassAuthPage] RegisterHttpRoutes called, _httpServer: {_httpServer != null}");
        
        if (_httpServer == null)
        {
            Debug.WriteLine("[HassAuthPage] _httpServer is null, cannot register routes");
            return;
        }
        
        UnregisterHttpRoutes();
        Debug.WriteLine("[HassAuthPage] Routes unregistered, now registering new routes");
        
        // 获取配置
        _httpServer.Get("/api/webview/config", async (req, res) =>
        {
            var hassUrl = _authStore != null ? await _authStore.GetHassUrlAsync() : null;
            string? remoteUrl = null;
            
            if (!string.IsNullOrEmpty(_httpServer.BaseUrl))
            {
                remoteUrl = _httpServer.BaseUrl.TrimEnd('/') + "/remote.html";
            }
            
            await res.Json(new { hassUrl, remoteUrl });
        });
        
        
        
        // 发现 Hass 实例（非阻塞，立即返回缓存结果）
        _httpServer.Get("/api/hass/discover", async (req, res) =>
        {
            // 立即返回缓存结果，不等待扫描完成
            await res.Json(_cachedHassInstances ?? new List<object>());
            
            // 如果正在扫描，触发一次刷新
            if (_isDiscovering && _cachedHassInstances == null)
            {
                StartBackgroundDiscovery();
            }
        });
        
        // 认证连接
        _httpServer.Post("/api/webview/auth", async (req, res) =>
        {
            var body = await req.JsonAsync<JsonNode>();
            var urlFromForm = body?["url"]?.GetValue<string>();
            
            if (string.IsNullOrEmpty(urlFromForm))
            {
                await res.Json(new { success = false, message = "请提供 Home Assistant URL" }, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            
            var auth = new HassAuth(urlFromForm);
            if (await auth.CheckApiStatusAsync())
            {
                if (_authStore != null)
                {
                    await _authStore.SetHassUrlAsync(auth.BaseUrl);
                }
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _state = PageState.InLoginFlow;
                    webView.WebViewControl.Source = auth.AuthorizeUri;
                });
                
                await res.Json(new { success = true, message = "正在跳转至认证页面..." });
            }
            else
            {
                await res.Json(new { success = false, message = "无法访问提供的 URL，请确保它是正确的 Home Assistant 实例地址。" }, System.Net.HttpStatusCode.BadRequest);
            }
        });
        
        Debug.WriteLine("[HassAuthPage] All HTTP routes registered successfully");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAuthPage();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnregisterHttpRoutes();
    }

    private void LoadAuthPage()
    {
        // 使用 localhost 加载 index.html（内部调用，不需要局域网 IP）
        var url = $"http://localhost:{_httpServer!.Port}/index.html";
        Debug.WriteLine($"[HassAuthPage] Loading auth page from HTTP: {url}");
        webView.WebViewControl.Source = url;
    }
    
    /// <summary>
    /// 在后台启动局域网扫描（非阻塞）
    /// </summary>
    private void StartBackgroundDiscovery()
    {
        if (_isDiscovering) return;
        
        _isDiscovering = true;
        
        // 使用 Task.Run 在后台执行扫描，不阻塞主线程
        Task.Run(async () =>
        {
            try
            {
                Debug.WriteLine("[HassAuthPage] Starting background Hass discovery...");
                var instances = await HassDiscovery.DiscoverAsync();
                _cachedHassInstances = instances.Cast<object>().ToList();
                Debug.WriteLine($"[HassAuthPage] Discovery completed, found {_cachedHassInstances.Count} instances");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassAuthPage] Background discovery error: {ex.Message}");
            }
            finally
            {
                _isDiscovering = false;
            }
        });
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

            if (_authStore == null)
            {
                ShowErrorAndStay("认证存储未初始化。");
                return;
            }

            var hassUrl = await _authStore.GetHassUrlAsync();
            var hassAuth = new HassAuth(hassUrl ?? string.Empty);
            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null)
            {
                ShowErrorAndStay("无法获取凭据，请重试。");
                return;
            }

            await _authStore.SetAccessTokenAsync(tokenResult.AccessToken ?? string.Empty);
            await _authStore.SetRefreshTokenAsync(tokenResult.RefreshToken ?? string.Empty);
            await _authStore.SetTokenExpiryUtcAsync(DateTime.UtcNow.AddSeconds(tokenResult.ExpiresIn));

            var hassApi = new HassRestApi(hassUrl ?? string.Empty, async (force) => 
            {
                if (_authStore == null) return string.Empty;
                return await _authStore.GetAccessTokenAsync() ?? string.Empty;
            });
            _hassApiService.Initialize(hassApi);

            var deviceId = await _authStore.GetDeviceIdAsync();
            var registrationRequest = new MobileAppRegistrationRequest
            {
                AppId = AppInfo.Current.PackageName,
                AppName = AppInfo.Current.Name,
                AppVersion = AppInfo.Current.VersionString,
                DeviceId = deviceId ?? string.Empty,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsName = DeviceInfo.Current.Platform.ToString(),
                OsVersion = DeviceInfo.Current.VersionString,
                SupportsEncryption = false,
                AppData = new MobileAppData(deviceId ?? string.Empty, _pageOptions.GetPushUrl?.Invoke() ?? string.Empty)
            };

            var registrationResult = await hassApi.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null)
            {
                ShowErrorAndStay("注册应用失败，请检查您的 Home Assistant 配置。");
                return;
            }

            await _authStore.SetWebhookIdAsync(registrationResult.WebhookId);

            // Authentication successful, close the modal page.
            IsAuthenticated = true;
            await MainThread.InvokeOnMainThreadAsync(() => Navigation.PopModalAsync());
        }
    }

    // Display an error toast and remain on the auth page.
    private void ShowErrorAndStay(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _state = PageState.NeedsAuth;
            LoadAuthPage();
            if (!string.IsNullOrEmpty(message)) webView.ShowToast(message);
        });
    }

    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    // Handle back press: go back in WebView if possible, otherwise close the modal.
    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        if (args.KeyName == "Back")
        {
            if (webView.WebViewControl.CanGoBack)
            {
                webView.WebViewControl.GoBack();
            }
            else
            {
                // 无法继续返回，关闭 Modal 让用户回到主页面
                MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
            }
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