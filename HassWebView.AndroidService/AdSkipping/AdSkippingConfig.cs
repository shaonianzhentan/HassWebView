
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
#if ANDROID
using HassWebView.AndroidService.Platforms.Android;
#endif

namespace HassWebView.AndroidService.AdSkipping
{
    public interface IAdSkippingConfig
    {
        string GetRuleUrl();
        Task SetRuleUrlAsync(string url);
    }

    public class AdSkippingConfig : IAdSkippingConfig
    {
        private const string RuleUrlKey = "AdSkipping_RuleUrl";

        public string GetRuleUrl()
        {
            return Preferences.Get(RuleUrlKey, string.Empty);
        }

        public async Task SetRuleUrlAsync(string url)
        {
            Preferences.Set(RuleUrlKey, url);
            LoadRules(url);
            await Task.CompletedTask;
        }

        internal static void LoadRules(string url)
        {
#if ANDROID
            if (!string.IsNullOrWhiteSpace(url))
            {
                // Fire-and-forget background task
                Task.Run(() => AdSkippingService.RuleManager.LoadRulesFromUrl(url));
            }
#endif
        }
    }
}
