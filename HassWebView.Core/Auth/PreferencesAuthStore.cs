using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace HassWebView.Core.Auth
{
    /// <summary>
    /// The default implementation of IAuthStore, using Microsoft.Maui.Storage.Preferences.
    /// </summary>
    public class PreferencesAuthStore : IAuthStore
    {
        private const string KeyDeviceId = "DeviceId";
        private const string KeyHassUrl = "HassUrl";
        private const string KeyWebhookId = "WebhookId";
        private const string KeyRefreshToken = "RefreshToken";
        private const string KeyAccessToken = "AccessToken";
        private const string KeyTokenExpiry = "TokenExpiryUtc";

        public Task<string> GetAccessTokenAsync() => Task.FromResult(Preferences.Get(KeyAccessToken, string.Empty));
        public Task SetAccessTokenAsync(string token) { Preferences.Set(KeyAccessToken, token); return Task.CompletedTask; }

        public Task<string> GetDeviceIdAsync()
        {
            var id = Preferences.Get(KeyDeviceId, string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                Preferences.Set(KeyDeviceId, id);
            }
            return Task.FromResult(id);
        }

        public Task<string> GetHassUrlAsync() => Task.FromResult(Preferences.Get(KeyHassUrl, string.Empty));
        public Task SetHassUrlAsync(string url) { Preferences.Set(KeyHassUrl, url); return Task.CompletedTask; }

        public Task<string> GetRefreshTokenAsync() => Task.FromResult(Preferences.Get(KeyRefreshToken, string.Empty));
        public Task SetRefreshTokenAsync(string token) { Preferences.Set(KeyRefreshToken, token); return Task.CompletedTask; }

        public Task<DateTime> GetTokenExpiryUtcAsync() => Task.FromResult(Preferences.Get(KeyTokenExpiry, DateTime.MinValue));
        public Task SetTokenExpiryUtcAsync(DateTime expiry) { Preferences.Set(KeyTokenExpiry, expiry); return Task.CompletedTask; }

        public Task<string> GetWebhookIdAsync() => Task.FromResult(Preferences.Get(KeyWebhookId, string.Empty));
        public Task SetWebhookIdAsync(string id) { Preferences.Set(KeyWebhookId, id); return Task.CompletedTask; }

        public Task ClearTokensAsync()
        {
            Preferences.Remove(KeyAccessToken);
            Preferences.Remove(KeyRefreshToken);
            Preferences.Remove(KeyWebhookId);
            Preferences.Remove(KeyTokenExpiry);
            // We are intentionally NOT removing KeyHassUrl here to preserve it for the next login.
            return Task.CompletedTask;
        }
    }
}
