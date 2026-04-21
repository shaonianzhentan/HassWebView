namespace HassWebView.Core.Controls;

public partial class WebViewWithCursor : ContentView
{
    public HassWebView WebView => wv;
    public Border Cursor => cursor;
    public AbsoluteLayout Root => root;
    public readonly KeyService _keyService;
    public readonly CursorControl _cursorControl;

    public WebViewWithCursor()
    {
        InitializeComponent();

        _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();

        if (_keyService != null)
        {
            cursor.IsVisible = true;
            _cursorControl = new CursorControl(cursor, root, wv);
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

    public void OnDoubleClick(string KeyName)
    {
        Debug.WriteLine($"[HassPage] Double Click: {KeyName}");
        if (_cursorControl is null) return false;
        switch (KeyName)
        {
            case "Enter": _cursorControl.DoubleClick(); break;
            case "Up": _cursorControl.SlideUp(); break;
            case "Down": _cursorControl.SlideDown(); break;
            case "Left": _cursorControl.SlideLeft(); break;
            case "Right": _cursorControl.SlideRight(); break;
        }
    }

    public bool OnLongClick(string KeyName)
    {
        Debug.WriteLine($"[HassPage] Long Click: {KeyName}");
        if (_keyService is null) return; 

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
