using HassWebView.AndroidService.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.ForegroundNotifications
{
    public static class ForegroundServiceExtensions
    {
        public static MauiAppBuilder UseForegroundNotifications(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            return builder;
        }
    }
}
