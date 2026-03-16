namespace HassWebView.AndroidService.NotificationForwarding
{
    /// <summary>
    /// Represents the data extracted from a forwarded notification.
    /// </summary>
    public class NotificationData
    {
        /// <summary>
        /// The package name of the app that posted the notification.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// The title of the notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The main text content of the notification.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The time when the notification was posted, in Unix milliseconds.
        /// </summary>
        public long PostTime { get; set; }

        /// <summary>
        /// The large icon of the notification, if available, as a byte array.
        /// This is typically the app icon or a contact photo.
        /// </summary>
        public byte[] LargeIcon { get; set; }

        /// <summary>
        /// The picture from a BigPictureStyle notification, if available, as a byte array.
        /// </summary>
        public byte[] Picture { get; set; }
    }
}
