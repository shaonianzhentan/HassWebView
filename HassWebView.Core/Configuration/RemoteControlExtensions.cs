using HassWebView.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;

#if ANDROID
using Android.App;
using HassWebView.Core.Platforms.Android;
#endif

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Handlers;
using Windows.System;
#endif

namespace HassWebView.Core.Configuration
{
    public static class RemoteControlExtensions
    {
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
                    android.OnApplicationCreating(app => 
                    {
                        var keyService = IPlatformApplication.Current.Services.GetService<KeyService>();
                        if (keyService == null)
                        {
                            Debug.WriteLine("[Critical Error] KeyService not found in DI container.");
                            return;
                        }

                        var callbackManager = new GlobalKeyCallbackManager(keyService);
                        app.RegisterActivityLifecycleCallbacks(callbackManager);
                    });
                });
#endif
            });

#if WINDOWS
            WindowHandler.Mapper.AppendToMapping("RemoteControl", (handler, view) =>
            {
                var keyService = handler.MauiContext?.Services.GetService<KeyService>();
                if (keyService == null || handler.PlatformView.Content is not UIElement ui) return;

                ui.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((s, e) => { e.Handled = keyService.OnPressed(e.Key.ToString()); }), true);
                ui.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler((s, e) => { keyService.OnReleased(); }), true);
            });
#endif

            return builder;
        }
    }
}
