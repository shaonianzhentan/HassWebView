
using System;
using System.Threading.Tasks;
using HassApi.Models;

namespace HassWebView.Core.Services
{
    public interface IHassAuthService
    {
        /// <summary>
        /// Processes the OAuth2 callback from Home Assistant, exchanges the authorization code for tokens,
        /// and registers the mobile app.
        /// </summary>
        /// <param name="callbackUri">The full callback URI containing the 'code' query parameter.</param>
        /// <param name="hassUrl">The base URL of the Home Assistant instance.</param>
        /// <param name="clientId">The Client ID for the OAuth2 application.</param>
        /// <param name="deviceId">The unique ID of the device.</param>
        /// <param name="pushUrl">The push notification URL for the device.</param>
        /// <returns>A redirect URI string for the success case, or null on failure.</returns>
        Task<string?> ProcessAuthorizationCallbackAsync(Uri callbackUri, string hassUrl, string clientId, string deviceId, string pushUrl);

        /// <summary>
        /// Checks if a valid authorization exists, attempts to refresh the access token,
        /// and updates the mobile app registration.
        /// </summary>
        /// <param name="deviceId">The unique ID of the device.</param>
        /// <param name="pushUrl">The push notification URL for the device.</param>
        /// <returns>The redirect URI on success, otherwise null.</returns>
        Task<string?> CheckAndRefreshAuthorizationAsync(string deviceId, string pushUrl);

        /// <summary>
        /// Refreshes the access token using the stored refresh token. This method is concurrency-safe.
        /// </summary>
        /// <returns>A TokenResult containing the new access token and its expiry, or null on failure.</returns>
        Task<TokenResult?> RefreshAccessTokenAsync();

        /// <summary>
        /// Clears all stored authentication tokens and related data.
        /// </summary>
        void Logout();
    }
}
