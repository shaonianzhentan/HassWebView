namespace Maui.HassWebView;

public class HassWebView : WebView
{
    public HassWebView()
    {
    }

    // 你可以加更多属性，例如注入 JSBridge
    public static readonly BindableProperty EnableZoomProperty =
        BindableProperty.Create(nameof(EnableZoom), typeof(bool), typeof(HassWebView), true);

    public bool EnableZoom
    {
        get => (bool)GetValue(EnableZoomProperty);
        set => SetValue(EnableZoomProperty, value);
    }
}
