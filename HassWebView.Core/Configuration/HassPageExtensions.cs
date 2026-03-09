using HassWebView.Core.Configuration;
using HassWebView.Core.Services; // 1. Add using statement
using HassWebView.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HassWebView.Core.Configuration
{
    public static class HassPageExtensions
    {
        public static MauiAppBuilder UseHassPage(this MauiAppBuilder builder, Action<HassPageOptions> configureOptions = null)
        {
            // Register HassApiService to share the HassRestApi instance
            builder.Services.TryAddSingleton<IHassApiService, HassApiService>(); // 2. Add service registration

            // Register HassPageOptions
            builder.Services.TryAddSingleton<HassPageOptions>(sp =>
            {
                var options = new HassPageOptions();
                configureOptions?.Invoke(options);
                return options;
            });

            // Register pages for dependency injection
            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder;
        }
    }
}
