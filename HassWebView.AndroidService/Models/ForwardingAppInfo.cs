namespace HassWebView.AndroidService.Models
{
    /// <summary>
    /// Represents information about an installed application for notification forwarding.
    /// </summary>
    public class ForwardingAppInfo
    {
        /// <summary>
        /// The user-friendly name of the application.
        /// </summary>
        public required string AppName { get; set; }

        /// <summary>
        /// The unique package name of the application (e.g., "com.google.android.gm").
        /// </summary>
        public required string PackageName { get; set; }

        /// <summary>
        /// A Base64 encoded string representing the application's icon.
        /// Can be converted to an ImageSource in the UI.
        /// </summary>
        public string? IconBase64 { get; set; }
    }
}
