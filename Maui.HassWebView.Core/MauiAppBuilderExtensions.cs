#if ANDROID
using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using Maui.HassWebView.Core.Platforms.Android.TencentX5;
#endif

using Microsoft.Maui.LifecycleEvents;

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

        builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android =>
                {
                    // Hook into the main activity's creation
                    android.OnCreate((activity, bundle) =>
                    {
                        QbSdk.DownloadWithoutWifi = true;

                        var tbsListener = new TencentTbsListener();
                        tbsListener.DownloadProgress += (s, e) =>
                        {
                            Console.WriteLine($"DownloadProgress {e}");
                        };
                        tbsListener.DownloadFinished += (s, e) =>
                        {
                            Console.WriteLine($"DownloadFinished {e}");
                        };
                        tbsListener.InstallFinished += (s, e) =>
                        {
                            Console.WriteLine($"InstallFinished {e}");
                        };

                        var preInitCallback = new PreInitCallback();
                        preInitCallback.CoreInitFinished += (s, e) =>
                        {
                            Console.WriteLine("CoreInitFinished");
                            //WebViewBtn_Clicked(null, null);
                        };
                        preInitCallback.ViewInitFinished += (s, e) =>
                        {
                            Console.WriteLine($"ViewInitFinished {e}");
                        };
                        QbSdk.SetTbsListener(tbsListener);

                        Console.WriteLine("InitX5Environment");
                        QbSdk.InitX5Environment(activity, preInitCallback);
                    });
                });
#endif
            });



        return builder;
    }
}