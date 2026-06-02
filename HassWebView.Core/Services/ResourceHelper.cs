using System.Collections.Concurrent;
using System.Reflection;
using HassWebView.Core; // Use the correct namespace for HassWebView

namespace HassWebView.Core.Services;

public static class ResourceHelper
{
    private static readonly ConcurrentDictionary<string, string> ResourceCache = new();
    private static readonly Assembly Assembly = typeof(ResourceHelper).GetTypeInfo().Assembly;

    public static async Task<string> GetResourceAsync(string relativePath)
    {
        if (ResourceCache.TryGetValue(relativePath, out var resourceContent))
        {
            return resourceContent;
        }

        var fullResourceName = $"HassWebView.Core.Resources.{relativePath.Replace("/", ".")}";

        using (var stream = Assembly.GetManifestResourceStream(fullResourceName))
        {
            if (stream == null)
            {
                throw new ArgumentException($"Embedded resource '{fullResourceName}' not found for relative path '{relativePath}'.", nameof(relativePath));
            }

            using (var reader = new StreamReader(stream))
            {
                resourceContent = await reader.ReadToEndAsync();
                ResourceCache.TryAdd(relativePath, resourceContent);
                return resourceContent;
            }
        }
    }

    public static async Task ExecuteScriptAsync(HassWebView webView, string scriptPath, string? functionCall = null)
    {
        if (webView == null) return;

        var scriptContent = await GetResourceAsync(scriptPath);
        var fullScript = functionCall == null ? scriptContent : $"{scriptContent}\n{functionCall};";
        
        if (string.IsNullOrEmpty(fullScript)) return;

        await MainThread.InvokeOnMainThreadAsync(() => webView.EvaluateJavaScriptAsync(fullScript));
    }
}
