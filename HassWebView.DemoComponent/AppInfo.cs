namespace HassWebView.DemoComponent;

using System.Reflection;

public static class AppInfo
{
    public static string AppName => Assembly.GetExecutingAssembly().GetName().Name ?? "HassWebView.DemoComponent";
    
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    
    public static string Framework => ".NET MAUI 10.0";
    
    public static string DesignSystem => "Home Assistant Design System";
    
    public static string GitHubUrl => "https://github.com/shaonianzhentan/HassWebView";
    
    public static int CompletedComponents => 8;
    
    public static int TotalComponents => 10;
}