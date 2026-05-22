using System.Net.Http;
using System.Text.Json;

namespace HassWebView.AndroidService.AdSkipping;

public class GkdRuleManager
{
    private static readonly HttpClient _httpClient = new();
    private List<GkdApp> _rules = [];

    /// <summary>Loads GKD rules from a remote URL.</summary>
    /// <param name="url">The URL of the rules file.</param>
    public async Task LoadRulesFromUrlAsync(string url)
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdSkipping] Error downloading GKD rules: {ex.Message}");
            ClearRules();
        }
    }

    /// <summary>Loads GKD rules from a JSON string (supports comments and trailing commas).</summary>
    /// <param name="jsonContent">Raw JSON content of the rules file.</param>
    public void LoadRulesFromString(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            ClearRules();
            return;
        }

        try
        {
            var docOptions = new JsonDocumentOptions
            {
                CommentHandling   = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            var serOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            using var doc = JsonDocument.Parse(jsonContent, docOptions);
            _rules = JsonSerializer.Deserialize<List<GkdApp>>(doc.RootElement.GetRawText(), serOptions) ?? [];
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdSkipping] Error parsing GKD rules: {ex.Message}");
            ClearRules();
        }
    }

    /// <summary>Clears all loaded ad-skipping rules.</summary>
    public void ClearRules() => _rules = [];

    /// <summary>
    /// Returns the rules for the given package name, or <see langword="null"/> if not found.
    /// </summary>
    public GkdApp? GetRulesForApp(string appId)
        => _rules.FirstOrDefault(app => app.Id == appId);
}
