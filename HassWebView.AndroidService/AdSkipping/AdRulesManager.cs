
using System.Net.Http;
#if ANDROID
using HassWebView.AndroidService.Platforms.Android;
#endif

namespace HassWebView.AndroidService.AdSkipping
{
    public interface IAdRulesManager
    {
        Task UpdateRulesFromUrlAsync(string url);
        Task LoadRulesFromLocalFileAsync();
    }

    public class AdRulesManager : IAdRulesManager
    {
        private const string LocalRulesFileName = "ad_skip_rules.json5";
        private static string LocalRulesPath => Path.Combine(FileSystem.AppDataDirectory, LocalRulesFileName);

        private readonly HttpClient _httpClient;

        public AdRulesManager()
        {
            _httpClient = new HttpClient();
        }

        public async Task UpdateRulesFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                // Optionally clear existing rules if the URL is cleared
                if (File.Exists(LocalRulesPath))
                {
                    File.Delete(LocalRulesPath);
                }
                #if ANDROID
                AdSkippingService.RuleManager.ClearRules();
                #endif
                return;
            }

            try
            {
                var rulesContent = await _httpClient.GetStringAsync(url);
                if (!string.IsNullOrWhiteSpace(rulesContent))
                {
                    await File.WriteAllTextAsync(LocalRulesPath, rulesContent);
                    // After successfully downloading and saving, load the new rules.
                    await LoadRulesFromLocalFileAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions for network errors, file writing errors, etc.
                Console.WriteLine($"[AdRulesManager] Error updating rules: {ex.Message}");
            }
        }

        public Task LoadRulesFromLocalFileAsync()
        {
            if (!File.Exists(LocalRulesPath))
            {
                return Task.CompletedTask;
            }
#if ANDROID
            var rulesContent = File.ReadAllText(LocalRulesPath);
            AdSkippingService.RuleManager.LoadRulesFromString(rulesContent);
#endif
            return Task.CompletedTask;
        }
    }
}
