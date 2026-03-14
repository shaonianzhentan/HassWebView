using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.Platforms.Android
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
            await LoadRulesAsync(url);
        }

        internal static async Task LoadRulesAsync(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                await AdSkippingService.RuleManager.LoadRulesFromUrl(url);
            }
        }
    }
}
