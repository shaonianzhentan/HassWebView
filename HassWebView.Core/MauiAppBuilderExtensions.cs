using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
using HassWebView.Core.Services;

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
    /// <summary>
    /// 使用 HassWebView 控件
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MauiAppBuilder UseHassWebView(this MauiAppBuilder builder)
    {
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
                            Console.WriteLine($"CoreInitFinished {e}");
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

                        controller.SystemBarsBehavior =
                            WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
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
        builder.Services.AddSingleton(new KeyService(longPressTimeout, doubleClickTimeout));

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, bundle) =>
                {
                    var keyService = (activity.Application as MauiApplication)?.Services.GetService<KeyService>();
                    if (keyService == null)
                    {
                        Debug.WriteLine("KeyService not found. Make sure it's registered.");
                        return;
                    }

                    var window = activity.Window;
                    var originalCallback = window.Callback;
                    window.Callback = new Platforms.Android.KeyCallback(originalCallback, keyService);
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
}