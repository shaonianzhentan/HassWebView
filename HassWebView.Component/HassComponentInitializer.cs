using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public class HassComponentInitializer : IMauiInitializeService
{
    private readonly HassComponentOptions _options;
    
    public HassComponentInitializer(HassComponentOptions options)
    {
        _options = options;
    }
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] Initialize called");
        
        try
        {
            var application = serviceProvider.GetService<Application>() ?? Application.Current;
            if (application?.Resources?.MergedDictionaries == null)
            {
                System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] ERROR: Application or Resources is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Application found. MergedDictionaries count: {application.Resources.MergedDictionaries.Count}");

            // 延迟初始化主题系统
            application.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] Calling ThemeManager.Initialize()");
                    // 使用配置的默认主题初始化（会自动从存储加载）
                    var defaultTheme = _options.DefaultTheme ?? ThemeMode.Dark;
                    ThemeManager.Initialize(defaultTheme);
                    System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] Theme system initialized successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Failed to initialize theme system: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Stack trace: {ex.StackTrace}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Initialize failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Stack trace: {ex.StackTrace}");
        }
    }
}