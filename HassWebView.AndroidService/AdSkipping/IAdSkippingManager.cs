namespace HassWebView.AndroidService.AdSkipping;

/// <summary>
/// Provides a unified interface to manage the ad-skipping feature,
/// including permission checks, rules management, and startup loading.
/// </summary>
public interface IAdSkippingManager
{
    /// <summary>Returns <see langword="true"/> when accessibility permission is granted.</summary>
    bool IsPermissionEnabled();

    /// <summary>Opens the system accessibility settings screen.</summary>
    void RequestPermission();

    /// <summary>
    /// Downloads rules from <paramref name="url"/>, saves them locally, and reloads.
    /// Passing null or empty clears all rules.
    /// </summary>
    Task UpdateRulesUrlAsync(string url);

    /// <summary>Returns the saved rules URL, or an empty string if not set.</summary>
    Task<string> GetRulesUrlAsync();

    /// <summary>Loads rules from local storage into the running service (called on startup).</summary>
    Task LoadRulesFromLocalFileAsync();
}
