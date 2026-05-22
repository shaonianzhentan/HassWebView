using System.Text.Json.Serialization;

namespace HassWebView.AndroidService.AdSkipping;

/// <summary>Represents a single application entry in a GKD subscription file.</summary>
public class GkdApp
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("groups")]
    public List<GkdGroup> Groups { get; set; } = [];
}

/// <summary>A group of rules for a specific scenario (e.g. "Splash Ad").</summary>
public class GkdGroup
{
    [JsonPropertyName("key")]
    public int Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    public List<GkdRule> Rules { get; set; } = [];
}

/// <summary>A single rule that identifies a UI element to click.</summary>
public class GkdRule
{
    [JsonPropertyName("matches")]
    public string Matches { get; set; } = string.Empty;

    [JsonPropertyName("excludeMatches")]
    public string? ExcludeMatches { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
