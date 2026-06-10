using HassWebView.Core.Configuration;
using HassWebView.Component.Models;

namespace HassWebView.Demo;

public partial class SettingsPage : ContentPage
{
    private readonly HassPageOptions _pageOptions;

    public string AppVersion => AppInfo.Current.VersionString;
    public string Platform => DeviceInfo.Current.Platform.ToString();
    public string DeviceName => DeviceInfo.Current.Name;
    public string Manufacturer => DeviceInfo.Current.Manufacturer;
    public string OsVersion => DeviceInfo.Current.VersionString;
    public string PushUrl => _pageOptions.GetPushUrl?.Invoke() ?? "未配置";
    public string HassUrl => _pageOptions.AuthStore?.GetHassUrlAsync().Result ?? "未授权";
    
    public string CurrentThemeText => ThemeManager.CurrentTheme switch
    {
        ThemeMode.Light => "浅色",
        ThemeMode.Dark => "深色",
        ThemeMode.System => "跟随系统",
        _ => "未知"
    };
    
    public string CurrentSizeText => SizeManager.CurrentSize switch
    {
        ComponentSize.Phone => "手机",
        ComponentSize.Tablet => "平板",
        ComponentSize.TV => "电视",
        _ => "未知"
    };

    public SettingsPage(HassPageOptions pageOptions)
    {
        InitializeComponent();
        _pageOptions = pageOptions;
        BindingContext = this;
        
        // 监听主题和尺寸变化
        ThemeManager.ThemeChanged += OnThemeChanged;
        SizeManager.SizeChanged += OnSizeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentThemeText));
    }
    
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentSizeText));
    }

    // Ensure we properly override Element.OnPropertyChanged to avoid hiding warnings from generated code
    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // 取消订阅事件
        ThemeManager.ThemeChanged -= OnThemeChanged;
        SizeManager.SizeChanged -= OnSizeChanged;
    }
}
