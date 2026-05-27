namespace HassWebView.Component;

using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Maui.ApplicationModel;

public partial class AppShell : Shell
{
    private Timer _navigationTimer;
    private List<string> _pageRoutes;
    private int _currentIndex = 0;

    public AppShell()
    {
        InitializeComponent();
        
        Loaded += OnAppShellLoaded;
    }

    private void OnAppShellLoaded(object sender, EventArgs e)
    {
        InitializePageRoutes();
        StartAutoNavigation();
    }

    private void InitializePageRoutes()
    {
        _pageRoutes = new List<string>
        {
            "MainPage",
            "AlertExamplePage",
            "ChipExamplePage",
            "ControlSwitchExamplePage",
            "InputExamplePage",
            "SpinnerExamplePage",
            "GaugeExamplePage",
            "ButtonCardExamplePage",
            "ButtonGridExamplePage",
            "DetailCardExamplePage",
            "DialogExamplePage",
            "EntityRowExamplePage",
            "EntityListGroupExamplePage",
            "SliderCardExamplePage",
            "StateBadgeExamplePage",
            "ThemeSelectorExamplePage",
            "SizeSelectorExamplePage",
            "SettingsGroupExamplePage",
            "AboutPage"
        };
    }

    private void StartAutoNavigation()
    {
        _navigationTimer = new Timer(3000);
        _navigationTimer.Elapsed += OnNavigationTimerElapsed;
        _navigationTimer.AutoReset = true;
        _navigationTimer.Start();
    }

    private void OnNavigationTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_currentIndex >= _pageRoutes.Count)
            {
                _currentIndex = 0;
            }

            try
            {
                var route = _pageRoutes[_currentIndex];
                System.Diagnostics.Debug.WriteLine($"导航到: {route} ({_currentIndex + 1}/{_pageRoutes.Count})");
                
                // await CurrentPage.DisplayAlert("导航测试", $"即将跳转到: {route}\n\n索引: {_currentIndex + 1}/{_pageRoutes.Count}", "确定");
                
                await Shell.Current.GoToAsync($"//{route}", animate: false);
                
                _currentIndex++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导航失败: {_pageRoutes[_currentIndex]} - {ex.Message}");
                await CurrentPage.DisplayAlert("导航失败", $"页面: {_pageRoutes[_currentIndex]}\n错误: {ex.Message}", "确定");
                _currentIndex++;
            }
        });
    }
}
