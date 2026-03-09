using HassWebView.Core.Configuration;
using HassWebView.Core.Views; // \u65b0\u589e using
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HassWebView.Core.Configuration
{
    public static class HassPageExtensions
    {
        public static MauiAppBuilder UseHassPage(this MauiAppBuilder builder, Action<HassPageOptions> configureOptions = null)
        {
            // 1. \u6ce8\u518c HassPageOptions
            builder.Services.TryAddSingleton<HassPageOptions>(sp =>
            {
                var options = new HassPageOptions();
                configureOptions?.Invoke(options);
                return options;
            });

            // 2. \u6ce8\u518c\u9875\u9762\u4f9d\u8d56\u6ce8\u5165 (\u65b0\u589e)
            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder;
        }
    }
}
