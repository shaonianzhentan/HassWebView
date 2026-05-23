namespace HassWebView.AndroidService.NotificationForwarding;

/// <summary>
/// Represents the data extracted from a forwarded notification.
/// </summary>
public class NotificationData
{
    /// <summary>The package name of the app that posted the notification.</summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>The title of the notification.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The main text content of the notification.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>The time when the notification was posted, in Unix milliseconds.</summary>
    public long PostTime { get; set; }

    /// <summary>
    /// The large icon of the notification as a PNG byte array, or null if unavailable.
    /// </summary>
    public byte[]? LargeIcon { get; set; }

    /// <summary>
    /// The picture from a BigPictureStyle notification as a PNG byte array, or null if unavailable.
    /// </summary>
    public byte[]? Picture { get; set; }
}
