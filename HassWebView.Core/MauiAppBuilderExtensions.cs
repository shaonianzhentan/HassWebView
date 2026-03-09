using Microsoft.Maui;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;

#if ANDROID
using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Platforms.Android.TencentX5;
#endif

namespace HassWebView.Core
{
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Registers the core services for the HassWebView component.
        /// This includes platform-specific handlers and initialization logic for the underlying web engine.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to add services to.</param>
        /// <returns>The <see cref="MauiAppBuilder"/> so that additional calls can be chained.</returns>
        public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder)
        {
            // Register platform-specific handlers for the HassWebView control
            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(HassWebView), typeof(Platforms.Android.HassWebViewHandler));
#elif WINDOWS
                handlers.AddHandler(typeof(HassWebView), typeof(Platforms.Windows.HassWebViewHandler));
#endif
            });

            // Configure platform-specific lifecycle events
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                // Android-specific logic to initialize the Tencent X5 browser engine
                events.AddAndroid(android =>
                {
                    android.OnApplicationCreate(app =>
                    {
                        // Initialize TBS settings for performance
                        QbSdk.InitTbsSettings(new Dictionary<string, Java.Lang.Object>
                        {
                            { TbsCoreSettings.TbsSettingsUseSpeedyClassloader, true },
                            { TbsCoreSettings.TbsSettingsUseDexloaderService, true }
                        });
                    });

                    android.OnCreate(async (activity, bundle) =>
                    {
                        // Setup TBS listener and initialize the X5 environment
                        QbSdk.DownloadWithoutWifi = true;
                        var tbsListener = new TencentTbsListener();
                        tbsListener.DownloadProgress += (s, e) => Debug.WriteLine($"[TBS] Download Progress: {e}");
                        tbsListener.DownloadFinished += (s, e) => Debug.WriteLine($"[TBS] Download Finished, Core Version: {e}");
                        tbsListener.InstallFinished += (s, e) => Debug.WriteLine($"[TBS] Install Finished, Core Version: {e}");

                        var preInitCallback = new PreInitCallback();
                        preInitCallback.CoreInitFinished += (s, e) => Debug.WriteLine("[TBS] Core Init Finished.");
                        preInitCallback.ViewInitFinished += (s, e) => Debug.WriteLine($"[TBS] View Init Finished, WasX5Core: {e}");

                        QbSdk.SetTbsListener(tbsListener);
                        QbSdk.InitX5Environment(activity, preInitCallback);
                    });
                });
#endif
            });

            return builder;
        }
    }
}
