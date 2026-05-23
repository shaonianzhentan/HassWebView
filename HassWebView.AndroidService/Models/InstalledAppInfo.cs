namespace HassWebView.AndroidService.Models;

/// <summary>
/// Represents an installed Android application.
/// </summary>
public class InstalledAppInfo
{
    /// <summary>App display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Package name (e.g. com.example.app).</summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>App icon as an ImageSource (used in MAUI UI).</summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// App icon as a Base64-encoded PNG string.
    /// Populated when the caller needs a serialisable icon representation (e.g. notification forwarding).
    /// </summary>
    public string? IconBase64 { get; set; }

    /// <summary>Whether the user has checked this app.</summary>
    public bool IsSelected { get; set; }
}
