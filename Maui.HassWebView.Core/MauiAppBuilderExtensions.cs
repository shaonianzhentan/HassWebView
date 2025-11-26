
namespace Maui.HassWebView.Core;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            // 使用非泛型重载，避免在编译期对 handler 类型做 IElementHandler 约束检查
            // handlers.AddHandler(typeof(HassWebView), typeof(HassWebViewHandler));
#if ANDROID
            handlers.AddHandler(typeof(HassWebView), typeof(Platforms.Android.HassWebViewHandler));
#elif WINDOWS
            handlers.AddHandler(typeof(HassWebView), typeof(Platforms.Windows.HassWebViewHandler));
#endif
        });

        return builder;
    }
}