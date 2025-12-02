using Android.App;
using Android.Util;
using Com.Tencent.Smtt.Sdk;
using Java.Util;

namespace HassWebView.Core.Platforms.Android.TencentX5;

public class WidgetClientFactory : Java.Lang.Object, IEmbeddedWidgetClientFactory
{
    private readonly Activity _context;
    private readonly DisplayMetrics _metrics;

    public WidgetClientFactory(Activity context)
    {
        _context = context;
        _metrics = context.Resources.DisplayMetrics;
    }

    public IEmbeddedWidgetClient CreateWidgetClient(
        string tag,
        IDictionary attributes,
        IEmbeddedWidget widget)
    {
        var attrs = new Dictionary<string, string>();

        var keys = attributes.KeySet().ToArray();
        foreach (var key in keys)
        {
            attrs[key.ToString()] = attributes.Get(key).ToString();
        }

        if (tag.Equals("video", StringComparison.OrdinalIgnoreCase))
            return new VideoWidget(tag, attrs, widget);

        if (tag.Equals("my-canvas", StringComparison.OrdinalIgnoreCase))
            return new CanvasWidget(tag, attrs, widget, _metrics);

        return null;
    }
}
