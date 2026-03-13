using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.AdSkipping
{
    public class GkdRuleManager
    {
        private readonly HttpClient _httpClient = new();
        private List<GkdApp> _rules = new();

        public async Task LoadRulesFromUrl(string url)
        {
            try
            {
                var json5 = await _httpClient.GetStringAsync(url);
                // Basic JSON5 to JSON conversion: remove comments and trailing commas
                var json = Regex.Replace(json5, @"//.*", ""); // remove single line comments
                json = Regex.Replace(json, @",(\s*[\]\}])", "$1"); // remove trailing commas

                _rules = JsonSerializer.Deserialize<List<GkdApp>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network error, parsing error)
                System.Diagnostics.Debug.WriteLine($"Error loading GKD rules: {ex.Message}");
                _rules = new List<GkdApp>(); // Ensure rules are empty on failure
            }
        }

        public GkdApp GetRulesForApp(string appId)
        {
            return _rules.FirstOrDefault(app => app.Id == appId);
        }
    }
}
