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
                "SettingsPage",
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
        _navigationTimer.Interval = TimeSpan.FromSeconds(4);
        _navigationTimer.Tick += OnNavigationTimerTick;
        _navigationTimer.Start();
    }

    private async void OnNavigationTimerTick(object? sender, EventArgs e)
        {
            if (_isNavigating)
            {
                Console.WriteLine($"导航超时检测: 上一个页面 {_pageRoutes[Math.Max(0, _currentIndex - 1)]} 可能卡住了");
                _isNavigating = false;
            }

            _navigationTimer?.Stop();
            _isNavigating = true;

            if (_currentIndex >= _pageRoutes.Count)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{timestamp}] 导航测试完成！已成功遍历所有 {_pageRoutes.Count} 个页面");
                _navigationTimer?.Stop();
                _isNavigating = false;
                return;
            }

            try
            {
                var route = _pageRoutes[_currentIndex];
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{timestamp}] 开始导航到: {route} ({_currentIndex + 1}/{_pageRoutes.Count})");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] 导航到: {route} ({_currentIndex + 1}/{_pageRoutes.Count})");
                
                await Shell.Current.GoToAsync($"//{route}", animate: false);
                
                timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{timestamp}] 成功导航到: {route}");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] 成功导航到: {route}");
                
                _currentIndex++;
            }
            catch (Exception ex)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"[{timestamp}] 导航失败: {_pageRoutes[_currentIndex]} - {ex}");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] 导航失败: {_pageRoutes[_currentIndex]} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] 异常堆栈: {ex.StackTrace}");
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