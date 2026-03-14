namespace HassWebView.AndroidService.Notifications
{
    public class NotificationAction
    {
        public NotificationAction(string title, string actionId, bool isAuthenticationRequired = false, bool isDestructive = false)
        {
            Title = title;
            ActionId = actionId;
            IsAuthenticationRequired = isAuthenticationRequired;
            IsDestructive = isDestructive;
        }

        public string Title { get; }
        public string ActionId { get; }
        public bool IsAuthenticationRequired { get; }
        public bool IsDestructive { get; }
    }
}
