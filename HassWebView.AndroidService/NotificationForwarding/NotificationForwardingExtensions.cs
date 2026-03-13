using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.NotificationForwarding
{
    public static class NotificationForwardingExtensions
    {
        public static MauiAppBuilder UseNotificationForwarding(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<INotificationForwardingManager, NotificationForwardingManager>();
            return builder;
        }
    }
}
