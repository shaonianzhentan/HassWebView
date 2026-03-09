using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HassWebView.Core.Auth;
using Microsoft.Maui.Controls;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HassWebView.Core.Configuration
{
    public class HassPageOptions
    {
        public IAuthStore AuthStore { get; set; }
        public Func<string, string?, Task> PlayVideo { get; set; }
        public Action ShowSettingsScreen { get; set; }
        public string PushUrl { get; set; }
        public Action<WebViewSource> SetWebViewSource { get; set; }

        /// <summary>
        /// Gets or sets the URL for the remote YAML configuration file.
        /// </summary>
        public string RemoteConfigsUrl { get; set; }

        /// <summary>
        /// Stores the parsed domain-specific configurations from the remote YAML file.
        /// The key is the domain name (e.g., "www.baidu.com").
        /// </summary>
        public Dictionary<string, WebViewDomainConfig> DomainConfigs { get; private set; } = new();

        public HassPageOptions()
        {
            AuthStore = new PreferencesAuthStore();
        }

        /// <summary>
        /// Downloads and parses the remote YAML configuration file.
        /// </summary>
        public async Task LoadRemoteConfigsAsync()
        {
            if (string.IsNullOrEmpty(RemoteConfigsUrl))
            {
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                var yamlContent = await httpClient.GetStringAsync(RemoteConfigsUrl);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var configs = deserializer.Deserialize<Dictionary<string, WebViewDomainConfig>>(yamlContent);

                if (configs != null)
                {
                    DomainConfigs = configs;
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                System.Diagnostics.Debug.WriteLine($"[HassPageOptions] Error loading remote configs: {ex.Message}");
            }
        }
    }
}
