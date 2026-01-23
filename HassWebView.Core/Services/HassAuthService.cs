
using HassApi;
using HassApi.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HassWebView.Core.Services
{
    public class HassAuthService : IHassAuthService
    {
        // Concurrency-safe mechanism for token refreshing
        private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        public async Task<string?> ProcessAuthorizationCallbackAsync(Uri callbackUri, string hassUrl, string clientId, string deviceId, string pushUrl)
        {
            var hassAuth = new HassAuth(hassUrl, clientId);
            if (!callbackUri.AbsoluteUri.StartsWith(hassAuth.RedirectUri))
            {
                return null;
            }

            var query = HttpUtility.ParseQueryString(callbackUri.Query);
            var code = query["code"];
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            var tokenResult = await hassAuth.GetRefreshTokenAsync(code);
            if (tokenResult == null) return null;

            await StoreTokensAsync(tokenResult.AccessToken, tokenResult.RefreshToken, tokenResult.ExpiresIn.ToString());
            await SecureStorage.SetAsync("HassUrl", hassUrl);
            await SecureStorage.SetAsync("ClientId", clientId);

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
            
            await SecureStorage.SetAsync("WebhookId", registrationResult.WebhookId);
            return Uri.EscapeDataString(hassAuth.RedirectUri);
        }

        public async Task<string?> CheckAndRefreshAuthorizationAsync(string deviceId, string pushUrl)
        {
            var hassUrl = await SecureStorage.GetAsync("HassUrl");
            var clientId = await SecureStorage.GetAsync("ClientId");
            var refreshToken = await SecureStorage.GetAsync("RefreshToken");
            var webhookId = await SecureStorage.GetAsync("WebhookId");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(webhookId) || string.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            var tokenResult = await RefreshAccessTokenAsync();
            if (tokenResult == null) return null;

            // Update device registration
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

            var hassAuth = new HassAuth(hassUrl, clientId);
            return Uri.EscapeDataString(hassAuth.RedirectUri);
        }

        public async Task<TokenResult?> RefreshAccessTokenAsync()
        {
            await _refreshSemaphore.WaitAsync();
            try
            {
                var hassUrl = await SecureStorage.GetAsync("HassUrl");
                var clientId = await SecureStorage.GetAsync("ClientId");
                var refreshToken = await SecureStorage.GetAsync("RefreshToken");

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(hassUrl) || string.IsNullOrEmpty(clientId))
                {
                    return null;
                }

                var hassAuth = new HassAuth(hassUrl, clientId);
                var result = await hassAuth.GetAccessTokenAsync(refreshToken);
                if (result == null) return null;
                
                await StoreTokensAsync(result.AccessToken, null, result.ExpiresIn.ToString()); // Only update access token and expiry

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

        public void Logout()
        {
            SecureStorage.RemoveAll();
            Debug.WriteLine("All stored authentication data cleared.");
        }

        private async Task StoreTokensAsync(string accessToken, string? refreshToken, string expiresIn)
        {
            await SecureStorage.SetAsync("AccessToken", accessToken);
            await SecureStorage.SetAsync("ExpiresIn", expiresIn);
            if (refreshToken != null)
            {
                await SecureStorage.SetAsync("RefreshToken", refreshToken);
            }
        }
    }
}
