using Android.Content;
using Android.Provider;
using HassWebView.AndroidService.AdSkipping;
using HassWebView.AndroidService.Platforms.Android;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http;

namespace HassWebView.AndroidService.Platforms.Android
{
    public class AdSkippingManager : IAdSkippingManager
    {
        private const string LocalRulesFileName = "ad_skip_rules.json5";
        private const string RulesUrlPreferenceKey = "AdSkipRulesUrl";

        private static string LocalRulesPath => Path.Combine(FileSystem.AppDataDirectory, LocalRulesFileName);
        private readonly HttpClient _httpClient = new();

        // Permission Logic
        public bool IsPermissionEnabled()
        {
            var context = Platform.AppContext;
            // Use the full name of the service type to avoid ambiguity
            var serviceName = $"{context.PackageName}/{typeof(HassWebView.AndroidService.Platforms.Android.AdSkippingService).FullName}";

            try
            {
                string settingValue = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.EnabledAccessibilityServices);
                return settingValue?.Contains(serviceName) ?? false;
            }
            catch (Settings.SettingNotFoundException)
            { 
                return false;
            }
        }

        public void RequestPermission()
        {
            var intent = new Intent(Settings.ActionAccessibilitySettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
        }

        // Rules Logic
        public async Task UpdateRulesUrlAsync(string url)
        {
            // Save the URL for future reference
            Preferences.Set(RulesUrlPreferenceKey, url);

            if (string.IsNullOrWhiteSpace(url))
            {
                // If URL is cleared, delete the local file and clear rules in the service
                if (File.Exists(LocalRulesPath))
                {
                    File.Delete(LocalRulesPath);
                }
                AdSkippingService.RuleManager.ClearRules();
                return;
            }

            try
            {
                var rulesContent = await _httpClient.GetStringAsync(url);
                if (!string.IsNullOrWhiteSpace(rulesContent))
                {
                    await File.WriteAllTextAsync(LocalRulesPath, rulesContent);
                    await LoadRulesFromLocalFileAsync(); // Reload rules after updating
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdSkippingManager] Error updating rules: {ex.Message}");
            }
        }

        public Task<string> GetRulesUrlAsync()
        {
            return Task.FromResult(Preferences.Get(RulesUrlPreferenceKey, string.Empty));
        }

        public Task LoadRulesFromLocalFileAsync()
        {            
            if (!File.Exists(LocalRulesPath))
            {
                // Ensure rules are cleared if the file doesn't exist
                AdSkippingService.RuleManager.ClearRules();
                return Task.CompletedTask;
            }

            var rulesContent = File.ReadAllText(LocalRulesPath);
            AdSkippingService.RuleManager.LoadRulesFromString(rulesContent);
            return Task.CompletedTask;
        }
    }
}
