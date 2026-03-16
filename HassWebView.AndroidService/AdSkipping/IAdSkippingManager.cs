using System.Threading.Tasks;

namespace HassWebView.AndroidService.AdSkipping
{
    /// <summary>
    /// Provides a unified interface to manage the ad skipping feature,
    /// including permission, rules, and status.
    /// </summary>
    public interface IAdSkippingManager
    {
        /// <summary>
        /// Checks if the accessibility permission is granted for the service.
        /// </summary>
        /// <returns>True if permission is granted, otherwise false.</returns>
        bool IsPermissionEnabled();

        /// <summary>
        /// Opens the system's accessibility settings screen to let the user grant permission.
        /// </summary>
        void RequestPermission();

        /// <summary>
        /// Updates the ad skipping rules from a given URL.
        /// The rules are downloaded, saved locally, and reloaded.
        /// If the URL is null or empty, existing rules will be cleared.
        /// </summary>
        /// <param name="url">The URL of the GKD rules file.</param>
        Task UpdateRulesUrlAsync(string url);

        /// <summary>
        /// Gets the URL of the currently configured ad skipping rules.
        /// </summary>
        /// <returns>The saved URL, or an empty string if not set.</returns>
        Task<string> GetRulesUrlAsync();

        /// <summary>
        /// Loads rules from local storage into the running service.
        /// This is typically called on app startup.
        /// </summary>
        Task LoadRulesFromLocalFileAsync();
    }
}
