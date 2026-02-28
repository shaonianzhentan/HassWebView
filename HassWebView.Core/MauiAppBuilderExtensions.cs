using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
using HassWebView.Core.Services;
using HassWebView.Core.Configuration;
using System.Runtime.InteropServices;


#if ANDROID
using Com.Tencent.Smtt.Export.External;
using Com.Tencent.Smtt.Sdk;
using HassWebView.Core.Platforms.Android.TencentX5;
using AndroidX.Core.View;
using Android.Runtime;
using Android.Views;
#endif

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Handlers;
using Windows.System;
#endif

namespace HassWebView.Core;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder, Action<HassWebViewOptions> setupAction = null)
    {
        var options = new HassWebViewOptions();
        setupAction?.Invoke(options);
        builder.Services.AddSingleton(options);

        builder.ConfigureMauiHandlers(handlers =>
        {
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
                android.OnApplicationCreate(app =>
                {
                    QbSdk.InitTbsSettings(new Dictionary<string, Java.Lang.Object>
                    {
                        { TbsCoreSettings.TbsSettingsUseSpeedyClassloader, true },
                        { TbsCoreSettings.TbsSettingsUseDexloaderService, true }
                    });

                });

                android.OnCreate(async (activity, bundle) =>
                {
                    QbSdk.DownloadWithoutWifi = true;
                    var tbsListener = new TencentTbsListener();
                    tbsListener.DownloadProgress += (s, e) => Console.WriteLine($"DownloadProgress {e}");
                    tbsListener.DownloadFinished += (s, e) => Console.WriteLine($"DownloadFinished {e}");
                    tbsListener.InstallFinished += (s, e) => Console.WriteLine($"InstallFinished {e}");

                    var preInitCallback = new PreInitCallback();
                    preInitCallback.CoreInitFinished += (s, e) => Console.WriteLine($"CoreInitFinished {e}");
                    preInitCallback.ViewInitFinished += (s, e) => Console.WriteLine($"ViewInitFinished {e}");
                    QbSdk.SetTbsListener(tbsListener);

                    Console.WriteLine("InitX5Environment");
                    QbSdk.InitX5Environment(activity, preInitCallback);
                    // 只在Android 10 以下执行下载内核操作
                    if (!OperatingSystem.IsAndroidVersionAtLeast(29))
                    {
                        string apkUrl = string.Empty;
                        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                        {
                            apkUrl = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm64_046295.tbs.apk";
                        }
                        else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                        {
                            apkUrl = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm_045912_x5.tbs.apk";
                        }
                        if (!string.IsNullOrEmpty(apkUrl))
                        {
                            Debug.WriteLine($"[ExternalBus] Initializing Tencent X5 Core with APK: {apkUrl}");
                            await TencentX5Service.InitializeX5CoreAsync(apkUrl);
                        }
                    }
                });
            });
#endif
        });

        return builder;
    }

    public static MauiAppBuilder UseImmersiveMode(this MauiAppBuilder builder)
    {
#if ANDROID
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, bundle) =>
                {
                    var window = activity.Window;
                    WindowCompat.SetDecorFitsSystemWindows(window, false);
                    var controller = WindowCompat.GetInsetsController(window, window.DecorView);
                    if (controller != null)
                    {
                        controller.Hide(WindowInsetsCompat.Type.StatusBars());
                        controller.Hide(WindowInsetsCompat.Type.NavigationBars());
                        controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
                    }
                });
            });
        });
#endif
        return builder;
    }

    public static MauiAppBuilder UseRemoteControl(
        this MauiAppBuilder builder,
        int longPressTimeout = 750,
        int doubleClickTimeout = 150)
    {
        // This already correctly registers the KeyService, so no changes needed here.
        builder.Services.AddSingleton(new KeyService(longPressTimeout, doubleClickTimeout));

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, bundle) =>
                {
                    var keyService = IPlatformApplication.Current.Services.GetService<KeyService>();
                    if (keyService == null)
                    {
                        Debug.WriteLine("[Critical Error] KeyService not found in DI container.");
                        return;
                    }

                    var window = activity.Window;
                    var originalCallback = window.Callback;
                    if (originalCallback is not Platforms.Android.KeyCallback)
                    {
                        window.Callback = new Platforms.Android.KeyCallback(originalCallback, keyService);
                    }
                });
            });
#endif
        });

#if WINDOWS
        WindowHandler.Mapper.AppendToMapping("RemoteControl", (handler, view) =>
        {
            var keyService = handler.MauiContext?.Services.GetService<KeyService>();
            if (keyService == null) return;

            if (handler.PlatformView.Content is not UIElement ui) return;
            
            ui.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((s, e) =>
            {
                bool shouldBeHandled = keyService.OnPressed(e.Key.ToString());
                e.Handled = shouldBeHandled;
            }), true);

            ui.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler((s, e) =>
            {
                keyService.OnReleased();
            }), true);
        });
#endif

        return builder;
    }

    public static MauiAppBuilder UseHttpServer(this MauiAppBuilder builder, int port, Action<HttpServer> setupRoutes = null)
    {
        builder.Services.AddSingleton<HttpServer>(serviceProvider =>
        {
            var httpServer = new HttpServer(HttpServer.GetLocalIPv4Address(), port);
            setupRoutes?.Invoke(httpServer);
            return httpServer;
        });

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(w => w
                .OnLaunched((window, args) =>
                {
                    var httpServer = IPlatformApplication.Current.Services.GetService<HttpServer>();
                    if (httpServer != null)
                    {
                        Task.Run(() => httpServer.StartAsync());
                    }
                })
                .OnClosed((window, args) =>
                {
                    var httpServer = IPlatformApplication.Current.Services.GetService<HttpServer>();
                    httpServer?.Stop();
                })
            );
#endif

#if ANDROID
            events.AddAndroid(a => a
                .OnCreate((activity, bundle) =>
                {
                    var httpServer = IPlatformApplication.Current.Services.GetService<HttpServer>();
                    if (httpServer != null)
                    {
                        Task.Run(() => httpServer.StartAsync());
                    }
                })
                .OnDestroy(activity =>
                {
                    var httpServer = IPlatformApplication.Current.Services.GetService<HttpServer>();
                    httpServer?.Stop();
                })
            );
#endif

        });
        return builder;
    }
}
