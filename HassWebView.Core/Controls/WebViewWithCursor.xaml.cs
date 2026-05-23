using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core.Controls;

public partial class WebViewWithCursor : ContentView
{
    public HassWebView WebViewControl => wv;
    public Border CursorControl => cursor;
    public AbsoluteLayout RootControl => root;
    public readonly KeyService _keyService;
    public readonly CursorControl _cursorControl;
    private readonly IRemoteControlService _remoteControlService;

    // Toast 相关
    private CancellationTokenSource _toastCts;
    private const int ToastDurationMs = 2500;
    private const uint ToastFadeMs = 200;

    public WebViewWithCursor()
    {
        InitializeComponent();

        _keyService = IPlatformApplication.Current.Services.GetService<KeyService>();
        _remoteControlService = IPlatformApplication.Current.Services.GetService<IRemoteControlService>();

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
        }

        wv.Navigating += OnNavigating;
        wv.Navigated += OnNavigated;

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    // Loading 进度条相关
    private CancellationTokenSource _loadingCts;

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _loadingCts?.Cancel();
            _loadingCts = new CancellationTokenSource();
            _ = RunLoadingBarAsync(_loadingCts.Token);
        });
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _loadingCts?.Cancel();
            _ = FinishLoadingBarAsync();
        });
    }

    private async Task RunLoadingBarAsync(CancellationToken token)
    {
        try
        {
            loadingBar.IsVisible = true;
            loadingBar.Opacity = 1;

            // 重置宽度为 0
            AbsoluteLayout.SetLayoutBounds(loadingBar, new Rect(0, 0, 0, 3));

            // 快速增长到 70%，然后缓慢爬到 90%（模拟等待响应）
            await GrowBarAsync(0.7, 300, token);
            await GrowBarAsync(0.9, 8000, token);
        }
        catch (OperationCanceledException) { /* 导航完成，由 FinishLoadingBarAsync 接管 */ }
    }

    private async Task GrowBarAsync(double targetRatio, uint durationMs, CancellationToken token)
    {
        const int steps = 30;
        var bounds = AbsoluteLayout.GetLayoutBounds(loadingBar);
        double startRatio = bounds.Width;
        double delta = targetRatio - startRatio;
        int stepDelay = (int)(durationMs / steps);

        for (int i = 1; i <= steps; i++)
        {
            token.ThrowIfCancellationRequested();
            double ratio = startRatio + delta * i / steps;
            AbsoluteLayout.SetLayoutBounds(loadingBar, new Rect(0, 0, ratio, 3));
            await Task.Delay(stepDelay, token);
        }
    }

    private async Task FinishLoadingBarAsync()
    {
        // 迅速填满到 100%
        AbsoluteLayout.SetLayoutBounds(loadingBar, new Rect(0, 0, 1, 3));
        await Task.Delay(150);
        // 淡出消失
        await loadingBar.FadeTo(0, 200);
        loadingBar.IsVisible = false;
        loadingBar.Opacity = 1;
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        _remoteControlService?.SetActiveControl(this);
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        _remoteControlService?.ClearActiveControl(this);
    }

    /// <summary>
    /// 显示 Toast 提示，自动在指定时间后淡出消失。
    /// 连续调用会取消上一次并立即显示新内容。
    /// </summary>
    public void ShowToast(string message, int durationMs = ToastDurationMs)
    {
        if (string.IsNullOrEmpty(message)) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // 取消上一个 Toast
            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();
            var token = _toastCts.Token;

            toastLabel.Text = message;
            toastPanel.IsVisible = true;

            // 淡入
            toastPanel.Opacity = 0;
            await toastPanel.FadeTo(1, ToastFadeMs);

            try
            {
                // 等待指定时间
                await Task.Delay(durationMs, token);

                // 淡出
                await toastPanel.FadeTo(0, ToastFadeMs);
                toastPanel.IsVisible = false;
            }
            catch (TaskCanceledException)
            {
                // 被新 Toast 取消，不做清理（新 Toast 会接管 UI）
            }
        });
    }

    public async Task LoadEmbeddedHtml(string resourcePath)
    {
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


    public void ToggleCursorVisibility()
    {
        cursor.IsVisible = !cursor.IsVisible;
    }


    public bool OnSingleClick(string KeyName)
    {
        Debug.WriteLine($"[HassPage] Single Click: {KeyName}");
        if (_cursorControl is null) return false;

        switch (KeyName)
        {
            case "Enter":
                _cursorControl.Click();
                break;
            case "Back":
                if (wv.CanGoBack) wv.GoBack();
                break;
            case "Up": _cursorControl.MoveUpBy(); break;
            case "Down": _cursorControl.MoveDownBy(); break;
            case "Left": _cursorControl.MoveLeftBy(); break;
            case "Right": _cursorControl.MoveRightBy(); break;
            default:
                return false;
        }

        return true;
    }

    public bool OnDoubleClick(string KeyName)
    {
        Debug.WriteLine($"[HassPage] Double Click: {KeyName}");
        if (_cursorControl is null) return false;
        
        switch (KeyName)
        {
            case "Enter":
                _ = _cursorControl.DoubleClick();
                return true;
            case "Up":
                _cursorControl.SlideUp();
                return true;
            case "Down":
                _cursorControl.SlideDown();
                return true;
            case "Left":
                _cursorControl.SlideLeft();
                return true;
            case "Right":
                _cursorControl.SlideRight();
                return true;
            default:
                return false;
        }
    }

    public bool OnLongClick(string KeyName)
    {
        Debug.WriteLine($"[HassPage] Long Click: {KeyName}");
        if (_keyService is null) return false; 

        const int repeatInterval = 100;
        switch (KeyName)
        {
            case "Up": _keyService.StartRepeatingAction(() => _cursorControl?.MoveUpBy(), repeatInterval); break;
            case "Down": _keyService.StartRepeatingAction(() => _cursorControl?.MoveDownBy(), repeatInterval); break;
            case "Left": _keyService.StartRepeatingAction(() => _cursorControl?.MoveLeftBy(), repeatInterval); break;
            case "Right": _keyService.StartRepeatingAction(() => _cursorControl.MoveRightBy(), repeatInterval); break;
            default:
                return false;
        }
        return true;
    }

}
