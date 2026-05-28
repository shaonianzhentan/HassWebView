using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace HassWebView.DemoComponent.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateCurrentSettings();
    }

    private void UpdateCurrentSettings()
    {
        // 更新当前主题显示
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        CurrentThemeLabel.Text = currentTheme == AppTheme.Dark ? "暗色模式" : currentTheme == AppTheme.Light ? "亮色模式" : "跟随系统";

        // 更新当前尺寸显示（简单实现）
        CurrentSizeLabel.Text = "默认尺寸";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCurrentSettings();
    }
}
