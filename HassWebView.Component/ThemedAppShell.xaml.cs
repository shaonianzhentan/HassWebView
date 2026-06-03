namespace HassWebView.Component;

using Microsoft.Maui.Controls;
using HassWebView.Component.Models;

/// <summary>
/// 支持主题切换的 Shell 基类
/// 提供自动响应主题变化的侧边栏和导航栏
/// </summary>
public partial class ThemedAppShell : Shell
{
    public ThemedAppShell()
    {
        InitializeComponent();
        
        // 设置默认的主题颜色
        UpdateShellColors();
        
        // 订阅主题变化事件
        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    /// <summary>
    /// 当主题变化时调用
    /// </summary>
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        // 在 UI 线程上更新 Shell 颜色
        if (Dispatcher.IsDispatchRequired)
        {
            Dispatcher.Dispatch(() => UpdateShellColors());
        }
        else
        {
            UpdateShellColors();
        }
    }

    /// <summary>
    /// 更新 Shell 的颜色以匹配当前主题
    /// </summary>
    private void UpdateShellColors()
    {
        // 使用 DynamicResource 让 MAUI 自动处理主题切换
        // FlyoutBackgroundColor 和 BackgroundColor 会通过 AppThemeBinding 自动更新
    }

    /// <summary>
    /// 判断当前是否为暗色主题
    /// </summary>
    private bool IsDarkTheme()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark ||
               (Application.Current?.UserAppTheme == AppTheme.Dark);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        
        // 清理事件订阅
        if (args.OldHandler != null)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
        }
    }
}