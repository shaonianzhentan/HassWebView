using Microsoft.Maui;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using AndroidX.Core.View;
#endif

namespace HassWebView.Core.Configuration
{
    public static class ImmersiveModeExtensions
    {
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
                            controller.Hide(WindowInsetsCompat.Type.StatusBars() | WindowInsetsCompat.Type.NavigationBars());
                            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
                        }
                    });
                });
            });
#endif
            return builder;
        }
    }
}
