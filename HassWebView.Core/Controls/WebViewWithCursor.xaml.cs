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

    // Alert 相关
    private TaskCompletionSource<bool> _alertTcs;
    private const uint AlertFadeMs = 200;
    private bool _isAlertVisible = false;
    private bool _isAlertCancelBtnFocused = false;

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

            AbsoluteLayout.SetLayoutBounds(loadingBar, new Rect(0, 0, 0, 3));

            await GrowBarAsync(0.7, 300, token);
            await GrowBarAsync(0.9, 8000, token);
        }
        catch (OperationCanceledException) { }
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
        AbsoluteLayout.SetLayoutBounds(loadingBar, new Rect(0, 0, 1, 3));
        await Task.Delay(150);
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
            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();
            var token = _toastCts.Token;

            toastLabel.Text = message;
            toastPanel.IsVisible = true;

            toastPanel.Opacity = 0;
            await toastPanel.FadeTo(1, ToastFadeMs);

            try
            {
                await Task.Delay(durationMs, token);
                await toastPanel.FadeTo(0, ToastFadeMs);
                toastPanel.IsVisible = false;
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    /// <summary>
    /// 显示 Alert 对话框（单按钮确认，类似 JavaScript 的 alert）。
    /// </summary>
    public async Task ShowAlert(string title, string message, string accept = "确定")
    {
        _alertTcs = new TaskCompletionSource<bool>();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            alertTitle.Text = title;
            alertMessage.Text = message;
            alertCancelBtn.IsVisible = false;
            alertAcceptBtn.Text = accept;

            alertOverlay.IsVisible = true;
            alertPanel.IsVisible = true;

            alertOverlay.Opacity = 0;
            alertPanel.Opacity = 0;

            _isAlertVisible = true;
            _isAlertCancelBtnFocused = false;

            _ = AnimateAlertInAsync();
        });

        await _alertTcs.Task;
    }

    /// <summary>
    /// 显示 Confirm 对话框（双按钮：取消和确定）。
    /// 返回 true 表示用户点击确定，false 表示取消。
    /// </summary>
    public async Task<bool> ShowConfirm(string title, string message, string cancel = "取消", string accept = "确定")
    {
        _alertTcs = new TaskCompletionSource<bool>();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            alertTitle.Text = title;
            alertMessage.Text = message;
            alertCancelBtn.Text = cancel;
            alertCancelBtn.IsVisible = true;
            alertAcceptBtn.Text = accept;

            alertOverlay.IsVisible = true;
            alertPanel.IsVisible = true;

            alertOverlay.Opacity = 0;
            alertPanel.Opacity = 0;

            _isAlertVisible = true;
            _isAlertCancelBtnFocused = true;

            _ = AnimateAlertInAsync();
        });

        return await _alertTcs.Task;
    }

    private async Task AnimateAlertInAsync()
    {
        await Task.WhenAll(
            alertOverlay.FadeTo(1, AlertFadeMs),
            alertPanel.FadeTo(1, AlertFadeMs)
        );
    }

    private async Task AnimateAlertOutAsync()
    {
        await Task.WhenAll(
            alertOverlay.FadeTo(0, AlertFadeMs),
            alertPanel.FadeTo(0, AlertFadeMs)
        );

        alertOverlay.IsVisible = false;
        alertPanel.IsVisible = false;
        _isAlertVisible = false;
        _isAlertCancelBtnFocused = false;
    }

    private async void OnAlertCancelClicked(object sender, EventArgs e)
    {
        await AnimateAlertOutAsync();
        _alertTcs?.SetResult(false);
    }

    private async void OnAlertAcceptClicked(object sender, EventArgs e)
    {
        await AnimateAlertOutAsync();
        _alertTcs?.SetResult(true);
    }

    private void UpdateAlertButtonFocus()
    {
        if (_isAlertCancelBtnFocused && alertCancelBtn.IsVisible)
        {
            alertCancelBtn.BackgroundColor = Color.FromHex("#E0E0E0");
            alertAcceptBtn.BackgroundColor = Color.FromHex("#F2F2F7");
        }
        else
        {
            alertCancelBtn.BackgroundColor = Color.FromHex("#F2F2F7");
            alertAcceptBtn.BackgroundColor = Color.FromHex("#007BFF");
        }
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

        // 如果对话框显示，遥控器控制对话框按钮
        if (_isAlertVisible)
        {
            return HandleAlertKeyPress(KeyName);
        }

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

    private bool HandleAlertKeyPress(string keyName)
    {
        switch (keyName)
        {
            case "Enter":
                // 按 Enter 键触发当前聚焦的按钮
                if (_isAlertCancelBtnFocused && alertCancelBtn.IsVisible)
                {
                    OnAlertCancelClicked(null, null);
                }
                else
                {
                    OnAlertAcceptClicked(null, null);
                }
                return true;
            case "Left":
            case "Right":
                // 左右键切换按钮焦点（仅 Confirm 对话框）
                if (alertCancelBtn.IsVisible)
                {
                    _isAlertCancelBtnFocused = !_isAlertCancelBtnFocused;
                    UpdateAlertButtonFocus();
                }
                return true;
            case "Back":
                // 按 Back 键关闭对话框（相当于取消）
                OnAlertCancelClicked(null, null);
                return true;
            default:
                return false;
        }
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