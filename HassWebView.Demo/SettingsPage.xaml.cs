using HassWebView.Core.Auth;
using HassWebView.Core.Configuration;

namespace HassWebView.Demo;

public partial class SettingsPage : ContentPage
{
    private readonly HassPageOptions _pageOptions;
    private readonly IAuthStore _authStore;

    public string AppVersion => AppInfo.Current.VersionString;
    public string Platform => DeviceInfo.Current.Platform.ToString();
    public string DeviceName => DeviceInfo.Current.Name;
    public string Manufacturer => DeviceInfo.Current.Manufacturer;
    public string OsVersion => DeviceInfo.Current.VersionString;
    public string PushUrl => _pageOptions.GetPushUrl?.Invoke() ?? "未配置";
    public string HassUrl => _authStore.GetHassUrlAsync().Result ?? "未授权";

    public SettingsPage(HassPageOptions pageOptions, IAuthStore authStore)
    {
        InitializeComponent();
        _pageOptions = pageOptions;
        _authStore = authStore;
        BindingContext = this;
    }
}
