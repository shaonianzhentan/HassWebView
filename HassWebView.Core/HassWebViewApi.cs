using System.Text.Json;
using System.Threading.Tasks;

namespace HassWebView.Core;

/// <summary>
/// Provides a C# interface to interact with the JavaScript APIs injected into the HassWebView.
/// </summary>
public class HassWebViewApi
{
    private readonly HassWebView _webView;

    internal HassWebViewApi(HassWebView webView)
    {
        _webView = webView;
    }

    /// <summary>
    /// Shows a toast notification inside the WebView.
    /// </summary>
    /// <param name="message">The message to display in the toast.</param>
    /// <param name="duration">The duration in milliseconds for the toast to be visible.</param>
    public Task ToastAsync(string message, int duration = 3000)
    {
        var messageJson = JsonSerializer.Serialize(message);
        var script = $"window.HassWebView.toast({messageJson}, {duration});";
        return _webView.EvaluateJavaScriptAsync(script);
    }

    /// <summary>
    /// Injects a block of CSS into the current page.
    /// </summary>
    /// <param name="css">The CSS content to inject.</param>
    /// <param name="host">An optional identifier for the injection source (for logging).</param>
    public Task InjectCssAsync(string css, string? host = null)
    { 
        var cssJson = JsonSerializer.Serialize(css);
        var hostJson = JsonSerializer.Serialize(host);
        var script = $"window.HassWebView.injectCss({cssJson}, {hostJson});";
        return _webView.EvaluateJavaScriptAsync(script);
    }

    /// <summary>
    /// Inserts text into the currently focused input element on the page.
    /// </summary>
    /// <param name="text">The text to insert.</param>
    /// <param name="append">If true, the text is appended; otherwise, it replaces the current content/selection.</param>
    public Task InsertTextAsync(string text, bool append = false)
    {
        var textJson = JsonSerializer.Serialize(text);
        var script = $"window.HassWebView.insertText({textJson}, {append.ToString().ToLower()});";
        return _webView.EvaluateJavaScriptAsync(script);
    }

    /// <summary>
    /// Simulates a key press event by calling the injected JavaScript function.
    /// </summary>
    /// <param name="key">The key to simulate (e.g., 'Tab', 'Enter').</param>
    public Task SimulateKeyPressAsync(string key)
    {
        var keyJson = JsonSerializer.Serialize(key);
        var script = $"window.HassWebView.simulateKeyPress({keyJson});";
        return _webView.EvaluateJavaScriptAsync(script);
    }
}
