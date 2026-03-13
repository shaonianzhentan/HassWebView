using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HassWebView.AndroidService.AdSkipping
{
    public class GkdRuleManager
    {
        private List<GkdApp> _rules = new();

        /// <summary>
        /// Loads GKD rules from a JSON5 string.
        /// </summary>
        /// <param name="json5Content">The string content of the rules file.</param>
        public void LoadRulesFromString(string json5Content)
        {
            if (string.IsNullOrWhiteSpace(json5Content))
            {
                ClearRules();
                return;
            }

            try
            {
                // Basic JSON5 to JSON conversion: remove comments and trailing commas
                var json = Regex.Replace(json5Content, @"//.*", ""); // remove single line comments
                json = Regex.Replace(json, @",(\s*[\]\}])", "$1"); // remove trailing commas

                _rules = JsonSerializer.Deserialize<List<GkdApp>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GkdApp>();
            }
            catch (Exception ex)
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
