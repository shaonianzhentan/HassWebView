using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.ApplicationModel;
using HassWebView.AndroidService.AdSkipping;

// Add a using alias to resolve the ambiguity between System.Action and Android.Views.Accessibility.Action
using Action = System.Action;

namespace HassWebView.AndroidService.AdSkipping
{
    public static class AdSkippingExtensions
    {
        public static MauiAppBuilder UseAdSkipping(this MauiAppBuilder builder)
        {
            // Register the single manager for the entire feature
            builder.Services.AddSingleton<IAdSkippingManager, AdSkippingManager>();

            // Register the startup service that uses the manager
            builder.Services.AddSingleton<Microsoft.Maui.Hosting.IMauiInitializeService, AdSkippingInitializeService>();

            return builder;
        }
    }
}
