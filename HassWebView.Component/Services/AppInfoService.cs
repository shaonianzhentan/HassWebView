namespace HassWebView.Component.Services;

using System.Reflection;

public class AppInfoService
{
    private static AppInfoService _instance;
    public static AppInfoService Instance => _instance ??= new AppInfoService();

    public string AppName => Assembly.GetExecutingAssembly().GetName().Name ?? "HassWebView.Component";
    
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    
    public string Framework => ".NET MAUI 10.0";
    
    public string DesignSystem => "Home Assistant Design System";
    
    public string GitHubUrl => "https://github.com/shaonianzhentan/HassWebView";
    
    public int CompletedComponents => 8;
    
    public int TotalComponents => 10;
}
