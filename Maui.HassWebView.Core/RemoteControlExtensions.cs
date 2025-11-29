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
#endif
using Microsoft.Maui;

namespace Maui.HassWebView.Core
{
    public static class RemoteControlExtensions
    {
        public static MauiAppBuilder UseRemoteControl(
            this MauiAppBuilder builder, 
            int longPressTimeout = 750, 
            int doubleClickTimeout = 300, 
            int downInterval = 100)
        {
            // 1. Register KeyService as a singleton with the provided timings
            builder.Services.AddSingleton(new KeyService(longPressTimeout, doubleClickTimeout, downInterval));

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android =>
                {
                    // Hook into the main activity's creation
                    android.OnCreate((activity, bundle) =>
                    {
                        var keyService = (activity.Application as MauiApplication)?.Services.GetService<KeyService>();
                        if (keyService == null)
                        {
                            Debug.WriteLine("KeyService not found. Make sure it's registered.");
                            return;
                        }

                        // Hijack the window's callback to intercept all key events
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
                if (keyService == null)
                {
                    Debug.WriteLine("KeyService not found in WindowHandler mapping.");
                    return;
                }

                if (handler.PlatformView.Content is not UIElement ui)
                {
                    Debug.WriteLine("Window content is not a UIElement.");
                    return;
                }
                
                // Intercept all KeyDown events
                ui.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((s, e) =>
                {
                    keyService.OnPressed(e.Key.ToString());
                    e.Handled = true; // Mark as handled to prevent default system behavior
                }), true); // handledEventsToo = true

                // Intercept all KeyUp events
                ui.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler((s, e) =>
                {
                    keyService.OnReleased();
                    e.Handled = true; // Mark as handled
                }), true); // handledEventsToo = true
            });
#endif

            return builder;
        }
    }
}
