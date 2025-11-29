#if ANDROID
using Android.Runtime;
using Android.Views;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Handlers;
using Windows.System;
#endif
using Microsoft.Maui;

namespace Maui.HassWebView.Core
{
    public static class RemoteControlExtensions
    {
        public static MauiAppBuilder UseRemoteControl(
            this MauiAppBuilder builder, 
            int longPressTimeout = 750, 
            int doubleClickTimeout = 200)
        {
            // 1. Register KeyService as a singleton with the provided timings
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
                    // --- THE NEW, CORRECT LOGIC ---
                    // 1. Call OnPressed and get its boolean result.
                    bool shouldBeHandled = keyService.OnPressed(e.Key.ToString());
                    
                    // 2. Assign the result directly to e.Handled.
                    e.Handled = shouldBeHandled;

                }), true);

                ui.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler((s, e) =>
                {
                    // KeyUp doesn't need to be conditional, as it only signals release.
                    // But we should still prevent it from bubbling up if the keydown was handled.
                    keyService.OnReleased();

                    // This part is tricky. We don't know if the original KeyDown was handled here.
                    // For simplicity and to avoid unintended side effects, we'll mark it as handled
                    // if it's not a key we'd typically let the system handle (like Volume keys).
                    if (e.Key != VirtualKey.VolumeUp && e.Key != VirtualKey.VolumeDown)
                    {
                         e.Handled = true;
                    }
                }), true);
            });
#endif

            return builder;
        }
    }
}
