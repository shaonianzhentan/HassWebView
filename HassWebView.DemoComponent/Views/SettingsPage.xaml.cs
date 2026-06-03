using Microsoft.Maui;
using Microsoft.Maui.Controls;
using HassWebView.Component.Models;

namespace HassWebView.DemoComponent.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateCurrentSettings();
        
        // 订阅主题变化事件
        ThemeManager.ThemeChanged += (s, e) => UpdateCurrentSettings();
    }

    private void UpdateCurrentSettings()
    {
        // 更新当前主题显示
        var currentTheme = ThemeManager.CurrentTheme;
        CurrentThemeLabel.Text = currentTheme switch
        {
            ThemeMode.Dark => "暗色模式",
            ThemeMode.Light => "亮色模式",
            ThemeMode.System => "跟随系统",
            _ => "未知"
        };

        // 更新当前尺寸显示（简单实现）
        CurrentSizeLabel.Text = SizeManager.CurrentSize.ToString();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCurrentSettings();
    }
}
