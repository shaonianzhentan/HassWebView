using HassWebView.Core.Configuration;

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

    public SettingsPage(HassPageOptions pageOptions)
    {
        InitializeComponent();
        _pageOptions = pageOptions;
        BindingContext = this;
    }
}
