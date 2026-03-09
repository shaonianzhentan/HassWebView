using System;
using System.Threading.Tasks;

namespace HassWebView.Core.Auth
{
    /// <summary>
    /// Defines an interface for storing and retrieving authentication data.
    /// This allows the default persistence mechanism (using MAUI Preferences) 
    /// to be replaced with a custom implementation.
    /// </summary>
    public interface IAuthStore
    {
        Task<string> GetDeviceIdAsync();

        Task<string> GetHassUrlAsync();
        Task SetHassUrlAsync(string url);

        Task<string> GetWebhookIdAsync();
        Task SetWebhookIdAsync(string id);

        Task<string> GetRefreshTokenAsync();
        Task SetRefreshTokenAsync(string token);

        Task<string> GetAccessTokenAsync();
        Task SetAccessTokenAsync(string token);

        Task<DateTime> GetTokenExpiryUtcAsync();
        Task SetTokenExpiryUtcAsync(DateTime expiry);

        /// <summary>
        /// Clears sensitive authentication tokens, typically on logout.
        /// </summary>
        Task ClearTokensAsync();
    }
}
