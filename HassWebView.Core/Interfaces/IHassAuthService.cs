using System;
using System.Threading.Tasks;
using HassApi.Models;

namespace HassWebView.Core.Interfaces
{
    public interface IHassAuthService
    {
        /// <summary>
        /// Processes the OAuth2 callback, exchanges the code for tokens, and registers the app.
        /// All successful authentication data is stored securely.
        /// </summary>
        Task<string> ProcessAuthorizationCallbackAsync(Uri callbackUri, string hassUrl, string clientId, string deviceId, string pushUrl);

        /// <summary>
        /// Checks if a valid authorization exists and attempts to refresh the access token.
        /// </summary>
        Task<string?> CheckAndRefreshAuthorizationAsync(string deviceId, string pushUrl);

        /// <summary>
        /// Refreshes the access token using the securely stored refresh token.
        /// </summary>
        Task<AuthorizationResult?> RefreshAccessTokenAsync();

        /// <summary>
        /// Retrieves the stored Home Assistant URL.
        /// </summary>
        /// <returns>The URL, or null if not found.</returns>
        Task<string?> GetHassUrlAsync();

        /// <summary>
        /// Retrieves the stored Client ID.
        /// </summary>
        /// <returns>The Client ID, or null if not found.</returns>
        Task<string?> GetClientIdAsync();

        /// <summary>
        /// Retrieves the stored Webhook ID.
        /// </summary>
        /// <returns>The Webhook ID, or null if not found.</returns>
        Task<string?> GetWebhookIdAsync();

        /// <summary>
        /// Clears all stored authentication data.
        /// </summary>
        void Logout();
    }
}
