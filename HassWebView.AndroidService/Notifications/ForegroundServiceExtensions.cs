using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.Notifications
{
    public static class ForegroundServiceExtensions
    {
        public static MauiAppBuilder UseForegroundService(this MauiAppBuilder builder, Action<INotificationService> configure)
        {
            var service = new NotificationService();
            configure?.Invoke(service);
            builder.Services.AddSingleton<INotificationService>(service);
            return builder;
        }
    }
}
