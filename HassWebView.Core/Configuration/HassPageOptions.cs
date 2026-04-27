using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using HassWebView.Core.Auth;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HassWebView.Core.Configuration
{
    public class HassPageOptions
    {
        public IAuthStore AuthStore { get; set; }
        public Func<string, string?, bool, Task> PlayVideo { get; set; }
        public Action ShowSettingsScreen { get; set; }
        public Action<string> OpenWebPage { get; set; }
        public string PushUrl { get; set; }

        private const string DomainConfigsCacheFile = "domain_configs.yaml";

        /// <summary>
        /// Stores the parsed domain-specific configurations.
        /// The key is the domain name (e.g., "www.baidu.com").
        /// </summary>
        public Dictionary<string, WebViewDomainConfig> DomainConfigs { get; private set; } = new();

        public HassPageOptions()
        {
            AuthStore = new PreferencesAuthStore();
            LoadConfigsFromCache();
        }

        private void LoadConfigsFromCache()
        {
            try
            {
                var cachePath = Path.Combine(FileSystem.AppDataDirectory, DomainConfigsCacheFile);
                if (File.Exists(cachePath))
                {
                    var yamlContent = File.ReadAllText(cachePath);
                    ParseAndSetConfigs(yamlContent);
                    Debug.WriteLine("[HassPageOptions] Loaded domain configs from cache.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassPageOptions] Error loading configs from cache: {ex.Message}");
            }
        }

        private void ParseAndSetConfigs(string yamlContent)
        {
            if (string.IsNullOrWhiteSpace(yamlContent)) return;

            try
            {
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
                 Debug.WriteLine($"[HassPageOptions] Error parsing YAML configs: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads and parses a remote YAML configuration file, then caches it.
        /// </summary>
        /// <param name="configUrl">The URL of the YAML configuration file.</param>
        public async Task LoadRemoteConfigsAsync(string configUrl)
        {
            if (string.IsNullOrEmpty(configUrl))
            {
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                var yamlContent = await httpClient.GetStringAsync(configUrl);
                
                // Parse and apply the new configs
                ParseAndSetConfigs(yamlContent);

                // Save the new content to cache
                var cachePath = Path.Combine(FileSystem.AppDataDirectory, DomainConfigsCacheFile);
                await File.WriteAllTextAsync(cachePath, yamlContent);
                Debug.WriteLine($"[HassPageOptions] Successfully downloaded and cached remote configs from {configUrl}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassPageOptions] Error loading remote configs from {configUrl}: {ex.Message}");
            }
        }
    }
}
