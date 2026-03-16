using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.AdSkipping
{
    public class GkdRuleManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private List<GkdApp> _rules = new();

        /// <summary>
        /// Loads GKD rules from a URL.
        /// </summary>
        /// <param name="url">The URL of the rules file.</param>
        public async Task LoadRulesFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ClearRules();
                return;
            }

            try
            {
                var jsonContent = await _httpClient.GetStringAsync(url);
                LoadRulesFromString(jsonContent);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdSkipping] Error downloading GKD rules from URL: {ex.Message}");
                ClearRules();
            }
        }

        /// <summary>
        /// Loads GKD rules from a JSON string with comments.
        /// </summary>
        /// <param name="jsonContent">The string content of the rules file.</param>
        public void LoadRulesFromString(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                ClearRules();
                return;
            }

            try
            {
                var jsonOptions = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                using (JsonDocument doc = JsonDocument.Parse(jsonContent, jsonOptions))
                {
                    _rules = JsonSerializer.Deserialize<List<GkdApp>>(doc.RootElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GkdApp>();
                }
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdSkipping] Error parsing GKD rules: {ex.Message}");
                ClearRules(); // Ensure rules are empty on failure
            }
        }

        /// <summary>
        /// Clears all loaded ad skipping rules.
        /// </summary>
        public void ClearRules()
        {
            _rules = new List<GkdApp>();
        }

        /// <summary>
        /// Gets the loaded rules for a specific application ID.
        /// </summary>
        /// <param name="appId">The package name of the application.</param>
        /// <returns>A GkdApp object containing the rules, or null if not found.</returns>
        public GkdApp GetRulesForApp(string appId)
        {            
            return _rules.FirstOrDefault(app => app.Id == appId);
        }
    }
}
