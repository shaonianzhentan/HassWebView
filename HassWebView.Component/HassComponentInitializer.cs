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
            
            // 如果 Application 或 Resources 还未初始化，延迟重试
            if (application?.Resources?.MergedDictionaries == null)
            {
                System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] Application or Resources not ready yet, retrying...");
                
                // 使用 Task.Delay 延迟重试，避免阻塞初始化流程
                _ = Task.Run(async () =>
                {
                    // 最多重试 5 次，每次间隔 100ms
                    for (int retry = 0; retry < 5; retry++)
                    {
                        await Task.Delay(100);
                        
                        application = Application.Current;
                        if (application?.Resources?.MergedDictionaries != null)
                        {
                            InitializeThemeSystem(application);
                            return;
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[HassComponentInitializer] Failed to initialize after retries");
                });
                
                return;
            }

            // 资源已就绪，直接初始化主题系统
            InitializeThemeSystem(application);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Initialize failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Stack trace: {ex.StackTrace}");
        }
    }
    
    private void InitializeThemeSystem(Application application)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] Application found. MergedDictionaries count: {application.Resources.MergedDictionaries.Count}");
            
            // 延迟初始化主题系统，确保 UI 线程就绪
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
            System.Diagnostics.Debug.WriteLine($"[HassComponentInitializer] InitializeThemeSystem failed: {ex.Message}");
        }
    }
}