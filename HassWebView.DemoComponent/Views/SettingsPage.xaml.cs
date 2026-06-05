using Microsoft.Maui;
using Microsoft.Maui.Controls;
using HassWebView.Component.Models;

namespace HassWebView.DemoComponent.Views;

public partial class SettingsPage : ContentPage
{
    private string? _currentThemeText;
    public string? CurrentThemeText
    {
        get => _currentThemeText;
        set
        {
            _currentThemeText = value;
            OnPropertyChanged();
        }
    }

    private string? _currentSizeText;
    public string? CurrentSizeText
    {
        get => _currentSizeText;
        set
        {
            _currentSizeText = value;
            OnPropertyChanged();
        }
    }

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this;
        UpdateCurrentSettings();
        
        ThemeManager.ThemeChanged += (s, e) => UpdateCurrentSettings();
    }

    private void UpdateCurrentSettings()
    {
        CurrentThemeText = ThemeManager.CurrentTheme switch
        {
            ThemeMode.Dark => "暗色模式",
            ThemeMode.Light => "亮色模式",
            ThemeMode.System => "跟随系统",
            _ => "未知"
        };

        CurrentSizeText = SizeManager.CurrentSize switch
        {
            ComponentSize.Phone => "手机",
            ComponentSize.Tablet => "平板",
            ComponentSize.TV => "电视",
            _ => "未知"
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCurrentSettings();
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new AboutPage());
    }
}