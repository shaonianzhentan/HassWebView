using HassWebView.Core.Services;
using HassWebView.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HassWebView.Core.Configuration
{
    public static class HassPageExtensions
    {
        public static MauiAppBuilder UseHassPage(this MauiAppBuilder builder, Action<IServiceProvider, HassPageOptions> configureOptions = null)
        {
            // Register HassApiService to share the HassRestApi instance
            builder.Services.TryAddSingleton<IHassApiService, HassApiService>();

            // Register HassPageOptions
            builder.Services.TryAddSingleton(sp =>
            {
                var options = new HassPageOptions();
                configureOptions?.Invoke(sp, options);
                return options;
            });

            // Register pages for dependency injection
            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder;
        }
    }
}
