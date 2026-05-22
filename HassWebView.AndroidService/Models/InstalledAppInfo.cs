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

    /// <summary>App icon as an ImageSource (loaded lazily).</summary>
    public ImageSource? Icon { get; set; }

    /// <summary>Whether the user has checked this app.</summary>
    public bool IsSelected { get; set; }
}
