using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HassWebView.AndroidService.AdSkipping
{
    // Represents a single application in the subscription file
    public class GkdApp
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("groups")]
        public List<GkdGroup> Groups { get; set; }
    }

    // Represents a group of rules for a specific scenario (e.g., "Splash Ad")
    public class GkdGroup
    {
        [JsonPropertyName("key")]
        public int Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rules")]
        public List<GkdRule> Rules { get; set; }
    }

    // Represents a single rule that identifies a UI element
    public class GkdRule
    {
        [JsonPropertyName("matches")]
        public string Matches { get; set; }

        [JsonPropertyName("excludeMatches")]
        public string ExcludeMatches { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }
    }
}
