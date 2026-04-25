using HassWebView.Core.Events;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core.Views;

public partial class HassMediaPage : ContentPage, IKeyHandler
{
    public string BaseUrl { get; set; }
    public string Url { get; set; }

    private readonly KeyService _keyService;
    private readonly IRemoteControlService _remoteControlService;

    // 状态：是否激活了光标控制模式
    private bool _isCursorModeActive = false;

    public HassMediaPage(KeyService keyService, IRemoteControlService remoteControlService)
    {
        InitializeComponent();
        _keyService = keyService;
        _remoteControlService = remoteControlService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // 页面出现时，将自己注册为当前的按键处理器
        _remoteControlService.SetActiveControl(webViewWithCursor);
        // 默认隐藏光标并进入视频控制模式
        webViewWithCursor.CursorControl.IsVisible = false;
        _isCursorModeActive = false;

        _ = LoadUrl(Url, BaseUrl);
    }

    protected override void OnDisappearing()
    {
        // 页面消失时，停止重复操作并释放按键处理器的控制权
        _keyService.StopRepeatingAction();
        _remoteControlService.ClearActiveControl(webViewWithCursor);
        base.OnDisappearing();
    }

    public async Task LoadUrl(string videoUrl, string baseUrl)
    {
        if (string.IsNullOrEmpty(videoUrl)) return;
        Debug.WriteLine($"Loading video URL: {videoUrl}");

        string htmlContent = await ResourceHelper.GetResourceAsync("MediaPlayer.html");

        var finalHtml = htmlContent.Replace("__HEIGHT__", webViewWithCursor.WebViewControl.Height.ToString())
                                     .Replace("__VIDEO_URL__", videoUrl);

        var uri = new Uri(videoUrl);
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = $"{uri.Scheme}://{uri.Host}/";
        }

        var htmlSource = new HtmlWebViewSource
        {
            BaseUrl = baseUrl,
            Html = finalHtml
        };

        Debug.WriteLine("Setting WebView source with HTML content from MediaPlayer.html.");
        webViewWithCursor.WebViewControl.Source = htmlSource;
    }

    private void VideoSeek(int second)
    {
        webViewWithCursor.WebViewControl.EvaluateJavaScriptAsync($"videoSeek({second})");
    }

    private void PlayPause()
    {
        webViewWithCursor.WebViewControl.EvaluateJavaScriptAsync("playPause()");
    }

    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown", "Menu" };

    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        Debug.WriteLine($"[HassMediaPage] Single Click: {args.KeyName}");

        if (args.KeyName == "Menu")
        {
            // 遵从您的建议，使用您封装好的方法来切换光标
            webViewWithCursor.ToggleCursorVisibility();
            // 然后同步内部状态以匹配光标的实际可见性
            _isCursorModeActive = webViewWithCursor.CursorControl.IsVisible;
            return; // Menu键只用于切换模式
        }

        // 如果处于光标模式，则将事件委托给 WebViewWithCursor 自己的处理器
        if (_isCursorModeActive)
        {
            webViewWithCursor.OnSingleClick(args.KeyName);
            return;
        }

        // 否则，在非光标模式下执行视频播放控制
        switch (args.KeyName)
        {
            case "Enter":
                PlayPause();
                break;

            case "Back":
                MainThread.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PopModalAsync());
                break;

            case "Left":
                VideoSeek(-5);
                break;

            case "Right":
                VideoSeek(5);
                break;
        }
    }

    public void OnLongClick(RemoteKeyEventArgs args)
    {
        Debug.WriteLine($"[HassMediaPage] Long Click: {args.KeyName}");

        // 如果处于光标模式，则将事件委托给 WebViewWithCursor 自己的处理器
        if (_isCursorModeActive)
        {
            webViewWithCursor.OnLongClick(args.KeyName);
            return;
        }
        
        // 否则，在非光标模式下执行视频播放控制
        const int repeatInterval = 100;
        switch (args.KeyName)
        {
            case "Left":
                _keyService.StartRepeatingAction(() => VideoSeek(-15), repeatInterval);
                break;
            case "Right":
                _keyService.StartRepeatingAction(() => VideoSeek(15), repeatInterval);
                break;
        }
    }

    #endregion
}
