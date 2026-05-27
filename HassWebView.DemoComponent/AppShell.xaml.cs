namespace HassWebView.DemoComponent;

using System;
using System.Collections.Generic;

public partial class AppShell : Shell
{
    private IDispatcherTimer? _navigationTimer;
    private List<string> _pageRoutes = new();
    private int _currentIndex = 0;
    private bool _isNavigating;

    public AppShell()
    {
        InitializeComponent();
        
        Loaded += OnAppShellLoaded;
        Unloaded += OnAppShellUnloaded;
    }

    private void OnAppShellLoaded(object? sender, EventArgs e)
    {
        InitializePageRoutes();
        StartAutoNavigation();
    }

    private void OnAppShellUnloaded(object? sender, EventArgs e)
    {
        _navigationTimer?.Stop();
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
        if (_navigationTimer is not null)
        {
            return;
        }

        _navigationTimer = Dispatcher.CreateTimer();
        _navigationTimer.Interval = TimeSpan.FromSeconds(3);
        _navigationTimer.Tick += OnNavigationTimerTick;
        _navigationTimer.Start();
    }

    private async void OnNavigationTimerTick(object? sender, EventArgs e)
    {
        if (_isNavigating)
        {
            return;
        }

        _navigationTimer?.Stop();
        _isNavigating = true;

        if (_currentIndex >= _pageRoutes.Count)
        {
            _currentIndex = 0;
        }

        try
        {
            var route = _pageRoutes[_currentIndex];
            Console.WriteLine($"Navigating to: {route} ({_currentIndex + 1}/{_pageRoutes.Count})");
            System.Diagnostics.Debug.WriteLine($"导航到: {route} ({_currentIndex + 1}/{_pageRoutes.Count})");
            
            // await CurrentPage.DisplayAlert("导航测试", $"即将跳转到: {route}\n\n索引: {_currentIndex + 1}/{_pageRoutes.Count}", "确定");
            
            await Shell.Current.GoToAsync($"//{route}", animate: false);
            Console.WriteLine($"Navigated to: {route}");
            
            _currentIndex++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation failed: {_pageRoutes[_currentIndex]} - {ex}");
            System.Diagnostics.Debug.WriteLine($"导航失败: {_pageRoutes[_currentIndex]} - {ex.Message}");
            await CurrentPage.DisplayAlert("导航失败", $"页面: {_pageRoutes[_currentIndex]}\n错误: {ex.Message}", "确定");
            _currentIndex++;
        }
        finally
        {
            _isNavigating = false;
            _navigationTimer?.Start();
        }
    }
}
