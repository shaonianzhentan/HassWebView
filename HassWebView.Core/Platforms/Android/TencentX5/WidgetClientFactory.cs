using Android.App;
using Android.Util;
using Com.Tencent.Smtt.Export.External.Embeddedwidget.Interfaces;
using Com.Tencent.Smtt.Sdk;
using Java.Util;
using Java.Util.Jar;
using Org.Json;
using System.Collections;
using System.Text.Json;

namespace HassWebView.Core.Platforms.Android.TencentX5;

using WebView = Com.Tencent.Smtt.Sdk.WebView;

public class WidgetClientFactory : Java.Lang.Object, IEmbeddedWidgetClientFactory
{
    private readonly DisplayMetrics _metrics;

    public WidgetClientFactory(WebView wv)
    {
        _metrics = wv.Context.Resources.DisplayMetrics;
    }

    public IEmbeddedWidgetClient CreateWidgetClient(
        string tag,
        IDictionary<string, string> attrs,
        IEmbeddedWidget widget)
    {
        if (tag.Equals("video", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var obj in attrs)
            {
                System.Diagnostics.Debug.WriteLine($"{obj.Key}: {obj.Value}");
            }
            
            if (attrs.ContainsKey("src") && attrs["src"].StartsWith("http"))
            {
                return new VideoWidget(tag, attrs["src"], widget);
            }
            if (attrs.ContainsKey("x5-source") && !string.IsNullOrEmpty(attrs["x5-source"]))
            {
                var x5source = attrs["x5-source"];
                dynamic dynamicList = JsonSerializer.Deserialize<dynamic>(x5source);
                foreach (var item in dynamicList)
                {
                    if (!string.IsNullOrEmpty(item.src))
                    {
                        return new VideoWidget(tag, attrs["src"], widget);
                    }
                }
            }
        }
        return null;
    }
}
