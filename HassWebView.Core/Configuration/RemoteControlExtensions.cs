using HassWebView.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using Android.App;
using HassWebView.Core.Platforms.Android;
#endif

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Handlers;
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

            builder.Services.AddSingleton<IRemoteControlService, RemoteControlService>();

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android =>
                {
                    KeyService? keyService = null;

                    android.OnCreate((activity, bundle) =>
                    {
                        var current = IPlatformApplication.Current;
                        if (current == null) return;
                        
                        keyService ??= current.Services.GetRequiredService<KeyService>();
                        var window = activity.Window;
                        if (window == null) return;
                        
                        var originalCallback = window.Callback;
                        if (originalCallback == null) return;
                        
                        window.Callback = new KeyCallback(originalCallback, keyService);
                    });

                    android.OnBackPressed(activity =>
                    {
                        return true;
                    });

                });
#endif
#if WINDOWS
                WindowHandler.Mapper.AppendToMapping("RemoteControl", (handler, view) =>
                {
                    var keyService = handler.MauiContext?.Services.GetService<KeyService>();
                    if (keyService == null || handler.PlatformView.Content is not UIElement ui) return;

                    ui.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((s, e) => { e.Handled = keyService.OnPressed(e.Key.ToString()); }), true);
                    ui.AddHandler(UIElement.KeyUpEvent, new KeyEventHandler((s, e) => { keyService.OnReleased(); }), true);
                });
#endif
            });

            return builder;
        }
    }
}
