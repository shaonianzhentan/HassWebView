using Microsoft.Maui;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
using HassWebView.Core.Configuration;

#if ANDROID
using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Platforms.Android.TencentX5;
using HassWebView.Core.Services;
#endif

namespace HassWebView.Core
{
    public static class MauiAppBuilderExtensions
    {
        // 静态字段，防止重复下载
        private static bool _x5DownloadStarted = false;
        private static readonly object _x5DownloadLock = new object();

        /// <summary>
        /// Registers the core services for the HassWebView component.
        /// This includes platform-specific handlers and initialization logic for the underlying web engine.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to add services to.</param>
        /// <returns>The <see cref="MauiAppBuilder"/> so that additional calls can be chained.</returns>
        public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder)
        {
            return UseHassWebView(builder, null);
        }

        /// <summary>
        /// Registers the core services for the HassWebView component.
        /// This includes platform-specific handlers and initialization logic for the underlying web engine.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to add services to.</param>
        /// <param name="configure">Optional. Configure HassWebView options and HttpServer routes.</param>
        /// <returns>The <see cref="MauiAppBuilder"/> so that additional calls can be chained.</returns>
        public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder, Action<HassWebViewOptions, Services.HttpServer>? configure = null)
        {
            // Create default options (default URLs are already set in property initializers)
            var options = new HassWebViewOptions();

            // 自动创建并注册 HttpServer（默认随机端口，智能选择IP）
            var httpServer = new Services.HttpServer();
            builder.Services.AddSingleton(httpServer);

            // Apply user configuration if provided (user can override defaults and configure routes)
            configure?.Invoke(options, httpServer);

            // 立即启动 HTTP 服务器（确保页面加载时服务器已就绪）
            _ = Task.Run(async () =>
            {
                try
                {
                    await httpServer.StartAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HassWebView] Failed to start HttpServer: {ex.Message}");
                }
            });

            // 在应用生命周期中管理服务器停止
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(a => a
                    .OnDestroy(activity => httpServer.Stop())
                );
#elif WINDOWS
                events.AddWindows(w => w
                    .OnClosed((window, args) => httpServer.Stop())
                );
#endif
            });
            
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

                    android.OnCreate((activity, bundle) =>
                    {
                        // Setup TBS listener and initialize the X5 environment
                        QbSdk.DownloadWithoutWifi = true;
                        var tbsListener = new TencentTbsListener();
                        tbsListener.DownloadProgress += (s, e) => Debug.WriteLine($"[TBS] Download Progress: {e}");
                        tbsListener.DownloadFinished += (s, e) => Debug.WriteLine($"[TBS] Download Finished, Core Version: {e}");
                        tbsListener.InstallFinished += (s, e) => Debug.WriteLine($"[TBS] Install Finished, Core Version: {e}");

                        var preInitCallback = new PreInitCallback();
                        preInitCallback.CoreInitFinished += (s, e) => Debug.WriteLine("[TBS] Core Init Finished.");
                        preInitCallback.ViewInitFinished += (s, e) =>
                        {
                            var isX5Core = e;
                            Debug.WriteLine($"[TBS] View Init Finished, WasX5Core: {isX5Core}");

                            // 如果不是 X5 内核且 Android 版本小于 9，则自动下载并安装
                            if (!isX5Core && HassWebViewOptions.ShouldDownloadX5Kernel())
                            {
                                // 防止重复下载
                                lock (_x5DownloadLock)
                                {
                                    if (_x5DownloadStarted)
                                    {
                                        Debug.WriteLine("[TBS] X5 download already started, skipping.");
                                        return;
                                    }
                                    _x5DownloadStarted = true;
                                }

                                var apkUrl = options.GetX5KernelUrl();
                                if (!string.IsNullOrEmpty(apkUrl))
                                {
                                    Debug.WriteLine($"[TBS] X5 core not available, starting auto-download from: {apkUrl}");
                                    _ = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            var result = await TencentX5Service.InitializeX5CoreAsync(apkUrl, progress =>
                                                {
                                                    Debug.WriteLine($"[TBS] X5 APK Download Progress: {progress}%");
                                                    options.OnX5DownloadProgress?.Invoke(progress);
                                                });
                                            Debug.WriteLine($"[TBS] X5 APK Download and Install {(result ? "Succeeded" : "Failed")}");
                                            options.OnX5DownloadCompleted?.Invoke(result);
                                            
                                            // 如果失败，允许重试
                                            if (!result)
                                            {
                                                lock (_x5DownloadLock)
                                                {
                                                    _x5DownloadStarted = false;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"[TBS] X5 APK Auto-download failed: {ex.Message}");
                                            options.OnX5DownloadCompleted?.Invoke(false);
                                            
                                            // 失败时允许重试
                                            lock (_x5DownloadLock)
                                            {
                                                _x5DownloadStarted = false;
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    Debug.WriteLine("[TBS] X5 core not available and no APK URL configured.");
                                    lock (_x5DownloadLock)
                                    {
                                        _x5DownloadStarted = false;
                                    }
                                }
                            }
                        };

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
