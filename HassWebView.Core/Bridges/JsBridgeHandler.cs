using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HassWebView.Core.Bridges
{
    // Data Transfer Object for messages from JavaScript
    public class JsBridgeMessage
    {
        public string BridgeName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public JsonElement[] Arguments { get; set; } = Array.Empty<JsonElement>();
    }

    // This class is now a helper specifically for the Windows implementation.
    public class JsBridgeHandler
    {
        private readonly IDictionary<string, object> _bridges;

        public JsBridgeHandler(IDictionary<string, object> bridges)
        {
            _bridges = bridges ?? new Dictionary<string, object>();
        }

        // The core method that handles the message using reflection
        public async Task HandleMessageAsync(string jsonMessage)
        {
            if (string.IsNullOrEmpty(jsonMessage))
                return;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = JsonSerializer.Deserialize<JsBridgeMessage>(jsonMessage, options);

                if (message == null || string.IsNullOrEmpty(message.BridgeName) || string.IsNullOrEmpty(message.MethodName))
                    return;

                if (!_bridges.TryGetValue(message.BridgeName, out var bridgeInstance))
                {
                    Console.WriteLine($"JsBridge Error: Bridge '{message.BridgeName}' not found.");
                    return;
                }

                var method = FindMethod(bridgeInstance.GetType(), message.MethodName, message.Arguments);

                if (method == null)
                {
                    Console.WriteLine($"JsBridge Error: Method '{message.MethodName}' with {message.Arguments.Length} arguments not found on bridge '{message.BridgeName}'.");
                    return;
                }

                var parameters = ConvertArguments(method.GetParameters(), message.Arguments);

                object? result = method.Invoke(bridgeInstance, parameters);

                if (result is Task task)
                {
                    await task;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling JsBridge message: {ex}");
            }
        }

        private MethodInfo? FindMethod(Type bridgeType, string methodName, JsonElement[] args)
        {
            // Find a method that matches name and parameter count. This is a simplification.
            // A more robust solution would check parameter types.
            return bridgeType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length == args.Length);
        }

        private object?[] ConvertArguments(ParameterInfo[] parameterInfos, JsonElement[] args)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true };
            var convertedArgs = new object?[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var paramType = parameterInfos[i].ParameterType;
                var jsonArg = args[i];
                convertedArgs[i] = JsonSerializer.Deserialize(jsonArg.GetRawText(), paramType, options);
            }
            return convertedArgs;
        }

        // Generates the JavaScript proxy code for the Windows platform.
        public string GenerateProxyScript()
        {
            if (!_bridges.Any())
                return string.Empty;

            var script = new StringBuilder();
            script.AppendLine("(function() {");
            script.AppendLine("    if (window.hasBridgeProxies) return;");
            script.AppendLine("    window.hasBridgeProxies = true;");

            foreach (var bridgeName in _bridges.Keys)
            {
                // This script creates a proxy that mimics the Android AddJavascriptInterface behavior.
                script.AppendLine($@"
    console.log('HassJsBridge: Creating Windows proxy for \'{bridgeName}\'.');
    window['{bridgeName}'] = new Proxy({{}}, {{
        get(target, propKey, receiver) {{
            return (...args) => {{
                const message = {{
                    BridgeName: '{bridgeName}',
                    MethodName: propKey,
                    Arguments: args
                }};

                if (window.chrome && window.chrome.webview) {{
                    window.chrome.webview.postMessage(message);
                }} else {{
                    console.error(`HassJsBridge: Windows native bridge (window.chrome.webview) not found for '{bridgeName}'.`);
                }}
            }};
        }}
    }}); ");
            }

            script.AppendLine("})();");
            return script.ToString();
        }
    }
}