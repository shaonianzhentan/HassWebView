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

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, EventArgs e)
    {
        _remoteControlService?.SetActiveControl(this);
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        _remoteControlService?.ClearActiveControl(this);
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
