using Android.Content;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http;

namespace HassWebView.AndroidService.AdSkipping;

public class AdSkippingManager : IAdSkippingManager
{
    private const string LocalRulesFileName    = "ad_skip_rules.json5";
    private const string RulesUrlPreferenceKey = "AdSkipRulesUrl";

    private static string LocalRulesPath => Path.Combine(FileSystem.AppDataDirectory, LocalRulesFileName);
    private readonly HttpClient _httpClient = new();

    // ── Permission ────────────────────────────────

    public bool IsPermissionEnabled()
    {
        var context     = Platform.AppContext;
        var serviceName = $"{context.PackageName}/{typeof(AdSkippingService).FullName}";
        try
        {
            var settingValue = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.EnabledAccessibilityServices);
            return settingValue?.Contains(serviceName) ?? false;
        }
        catch (Settings.SettingNotFoundException)
        {
            return false;
        }
    }

    public void RequestPermission()
    {
        var intent = new Intent(Settings.ActionAccessibilitySettings);
        intent.AddFlags(ActivityFlags.NewTask);
        Platform.AppContext.StartActivity(intent);
    }

    // ── Rules ─────────────────────────────────────

    public async Task UpdateRulesUrlAsync(string url)
    {
        Preferences.Set(RulesUrlPreferenceKey, url);

        if (string.IsNullOrWhiteSpace(url))
        {
            if (File.Exists(LocalRulesPath))
                File.Delete(LocalRulesPath);
            AdSkippingService.RuleManager.ClearRules();
            return;
        }

        try
        {
            var rulesContent = await _httpClient.GetStringAsync(url);
            if (!string.IsNullOrWhiteSpace(rulesContent))
            {
                await File.WriteAllTextAsync(LocalRulesPath, rulesContent);
                await LoadRulesFromLocalFileAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdSkippingManager] Error updating rules: {ex.Message}");
        }
    }

    public Task<string> GetRulesUrlAsync()
        => Task.FromResult(Preferences.Get(RulesUrlPreferenceKey, string.Empty));

    public async Task LoadRulesFromLocalFileAsync()
    {
        if (!File.Exists(LocalRulesPath))
        {
            AdSkippingService.RuleManager.ClearRules();
            return;
        }

        var rulesContent = await File.ReadAllTextAsync(LocalRulesPath);
        AdSkippingService.RuleManager.LoadRulesFromString(rulesContent);
    }
}
