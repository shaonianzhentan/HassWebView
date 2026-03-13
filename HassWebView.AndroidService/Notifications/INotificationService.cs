using System.Collections.Generic;

namespace HassWebView.AndroidService.Notifications
{
    public interface INotificationService
    {
        void ShowNotification(string title, string content, int notificationId, List<NotificationAction> actions);
        void CancelNotification(int notificationId);
    }
}
