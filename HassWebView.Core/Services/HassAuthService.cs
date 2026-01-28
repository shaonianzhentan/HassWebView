
using HassApi;
using HassApi.Models;
using HassWebView.Core.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;

namespace HassWebView.Core.Services
{
    public class HassAuthService : IHassAuthService
    {
        // Define keys for SecureStorage
        private const string HassUrlKey = "HassUrl";
        private const string ClientIdKey = "ClientId";
        private const string WebhookIdKey = "WebhookId";
        private const string AccessTokenKey = "AccessToken";
        private const string RefreshTokenKey = "RefreshToken";
        private const string ExpiresInKey = "ExpiresIn";

        private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        public async Task<string> ProcessAuthorizationCallbackAsync(Uri callbackUri, string hassUrl, string clientId, string deviceId, string pushUrl)
        {
            var hassAuth = new HassAuth(hassUrl, clientId);
            if (!callbackUri.AbsoluteUri.StartsWith(hassAuth.RedirectUri))
            {
                return null;
            }

            var query = HttpUtility.ParseQueryString(callbackUri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code)) return string.Empty;

            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null) return string.Empty;

            // Store tokens and identifiers
            await SecureStorage.SetAsync(AccessTokenKey, tokenResult.AccessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, tokenResult.RefreshToken);
            await SecureStorage.SetAsync(ExpiresInKey, tokenResult.ExpiresIn.ToString());
            await SecureStorage.SetAsync(HassUrlKey, hassUrl);
            await SecureStorage.SetAsync(ClientIdKey, clientId);

            var hassClient = new HassClient(hassUrl, tokenResult.AccessToken);
            var registrationRequest = new MobileAppRegistrationRequest
            {
                AppId = AppInfo.Current.PackageName,
                AppName = AppInfo.Current.Name,
                AppVersion = AppInfo.Current.VersionString,
                DeviceId = deviceId,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsName = DeviceInfo.Current.Platform.ToString(),
                OsVersion = DeviceInfo.Current.VersionString,
                SupportsEncryption = false,
                AppData = new MobileAppData(deviceId, pushUrl)
            };

            var registrationResult = await hassClient.RegisterMobileAppAsync(registrationRequest);
            if (registrationResult?.WebhookId == null) return null;

            await SecureStorage.SetAsync(WebhookIdKey, registrationResult.WebhookId);
            return hassAuth.RedirectUri;
        }

        public async Task<string?> CheckAndRefreshAuthorizationAsync(string deviceId, string pushUrl)
        {
            var hassUrl = await GetHassUrlAsync();
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            var webhookId = await GetWebhookIdAsync();

            if (string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(webhookId))
            {
                return null;
            }

            var tokenResult = await RefreshAccessTokenAsync();
            if (tokenResult == null) return null;

            var mobileApp = new MobileApp(hassUrl, webhookId);
            await mobileApp.UpdateRegistrationAsync(new UpdateRegistrationRequest
            {
                AppVersion = AppInfo.Current.VersionString,
                DeviceName = $"{DeviceInfo.Current.Platform} {DeviceInfo.Name}",
                Model = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsVersion = DeviceInfo.Current.VersionString,
                AppData = new MobileAppData(deviceId, pushUrl)
            });

            return new HassAuth(hassUrl, await GetClientIdAsync()).RedirectUri;
        }

        public async Task<AuthorizationResult> RefreshAccessTokenAsync()
        {
            await _refreshSemaphore.WaitAsync();
            try
            {
                var hassUrl = await GetHassUrlAsync();
                var clientId = await GetClientIdAsync();
                var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(clientId))
                {
                    return null;
                }

                var hassAuth = new HassAuth(hassUrl, clientId);
                var result = await hassAuth.GetAccessTokenAsync(refreshToken);
                if (result == null) return null;
                
                await SecureStorage.SetAsync(AccessTokenKey, result.AccessToken);
                await SecureStorage.SetAsync(ExpiresInKey, result.ExpiresIn.ToString());

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing access token: {ex.Message}");
                return null;
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }
        
        public Task<string?> GetHassUrlAsync() => SecureStorage.GetAsync(HassUrlKey);

        public Task<string?> GetClientIdAsync() => SecureStorage.GetAsync(ClientIdKey);

        public Task<string?> GetWebhookIdAsync() => SecureStorage.GetAsync(WebhookIdKey);

        public void Logout()
        {
            SecureStorage.Remove(HassUrlKey);
            SecureStorage.Remove(ClientIdKey);
            SecureStorage.Remove(WebhookIdKey);
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(ExpiresInKey);
            Debug.WriteLine("Specific authentication data cleared.");
        }
    }
}
