using HassWebView.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System;
using System.Threading.Tasks;

namespace HassWebView.Core.Configuration
{
    public static class HttpServerExtensions
    {
        public static MauiAppBuilder UseHttpServer(this MauiAppBuilder builder, int port, Action<IServiceProvider, HttpServer> setupRoutes = null)
        {
            builder.Services.AddSingleton<HttpServer>(serviceProvider =>
            {
                var httpServer = new HttpServer(HttpServer.GetLocalIPv4Address(), port);
                // Pass the serviceProvider and the httpServer to the setup action
                setupRoutes?.Invoke(serviceProvider, httpServer);
                return httpServer;
            });

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(a => a
                    .OnCreate((activity, bundle) => Task.Run(() => IPlatformApplication.Current.Services.GetService<HttpServer>()?.StartAsync()))
                    .OnDestroy(activity => IPlatformApplication.Current.Services.GetService<HttpServer>()?.Stop())
                );
#endif
#if WINDOWS
                events.AddWindows(w => w
                    .OnLaunched((window, args) => Task.Run(() => IPlatformApplication.Current.Services.GetService<HttpServer>()?.StartAsync()))
                    .OnClosed((window, args) => IPlatformApplication.Current.Services.GetService<HttpServer>()?.Stop())
                );
#endif
            });
            return builder;
        }
    }
}
