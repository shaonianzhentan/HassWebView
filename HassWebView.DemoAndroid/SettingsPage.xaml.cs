using System.ComponentModel;
using System.Runtime.CompilerServices;
using HassWebView.AndroidService;

namespace HassWebView.DemoAndroid;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged
{
    // ─────────────────────────────────────────────
    // Preferences keys
    // ─────────────────────────────────────────────
    private const string KeyGpsInterval         = "gps_interval";
    private const string KeyGpsWifiStop         = "gps_wifi_stop";
    private const string KeyScreenEvent         = "screen_event_push";
    private const string KeySmsForward          = "sms_forward";
    private const string KeyWidgetAutoRefresh   = "widget_auto_refresh";
    private const string KeyWidgetRefreshMins   = "widget_refresh_minutes";
    private const string KeyKeyControl          = "key_control";
    private const string KeyLanAutoSwitch       = "lan_auto_switch";

    // ─────────────────────────────────────────────
    // INotifyPropertyChanged
    // ─────────────────────────────────────────────
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(name);
    }

    // ─────────────────────────────────────────────
    // Bindable state properties
    // ─────────────────────────────────────────────

    private string _gpsPermissionStatus = "检查中…";
    public string GpsPermissionStatus
    {
        get => _gpsPermissionStatus;
        private set => Set(ref _gpsPermissionStatus, value);
    }

    private bool _gpsGranted;
    public bool GpsGranted
    {
        get => _gpsGranted;
        private set { Set(ref _gpsGranted, value); OnPropertyChanged(nameof(GpsNotGranted)); }
    }
    public bool GpsNotGranted => !_gpsGranted;

    private string _notifyPermissionStatus = "检查中…";
    public string NotifyPermissionStatus
    {
        get => _notifyPermissionStatus;
        private set => Set(ref _notifyPermissionStatus, value);
    }

    private string _notifyAppCount = "未选择";
    public string NotifyAppCount
    {
        get => _notifyAppCount;
        private set => Set(ref _notifyAppCount, value);
    }

    private string _smsPermissionStatus = "检查中…";
    public string SmsPermissionStatus
    {
        get => _smsPermissionStatus;
        private set => Set(ref _smsPermissionStatus, value);
    }

    private bool _smsGranted;
    public bool SmsGranted
    {
        get => _smsGranted;
        private set { Set(ref _smsGranted, value); OnPropertyChanged(nameof(SmsNotGranted)); }
    }
    public bool SmsNotGranted => !_smsGranted;

    private string _widgetCount = "0 个";
    public string WidgetCount
    {
        get => _widgetCount;
        private set => Set(ref _widgetCount, value);
    }

    // ─────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this;
        RestoreSwitchStates();
    }

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshAllStatusAsync();
    }

    // ─────────────────────────────────────────────
    // Permission checks
    // ─────────────────────────────────────────────

    private async Task RefreshAllStatusAsync()
    {
        await CheckGpsPermissionAsync();
        await CheckNotifyPermissionAsync();
        await CheckSmsPermissionAsync();
        RefreshNotifyAppCount();
        RefreshWidgetCount();
    }

    private async Task CheckGpsPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        GpsGranted = status == PermissionStatus.Granted;
        GpsPermissionStatus = GpsGranted ? "已授权" : "未授权";
    }

    private Task CheckNotifyPermissionAsync()
    {
        var granted = AndroidPermissionHelper.AreNotificationsEnabled();
        NotifyPermissionStatus = granted ? "已开启" : "未开启";
        return Task.CompletedTask;
    }

    private async Task CheckSmsPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
        SmsGranted = status == PermissionStatus.Granted;
        SmsPermissionStatus = SmsGranted ? "已授权" : "未授权";
    }

    private void RefreshNotifyAppCount()
    {
        var count = AppSelectPage.GetSelectedCount();
        NotifyAppCount = count > 0 ? $"已选 {count} 个" : "点击选择";
    }

    private void RefreshWidgetCount()
    {
        // Placeholder — replace with actual widget query when widgets are implemented
        WidgetCount = "0 个";
    }

    // ─────────────────────────────────────────────
    // Restore switch states from Preferences
    // ─────────────────────────────────────────────

    private void RestoreSwitchStates()
    {
        GpsIntervalSlider.Value        = Preferences.Get(KeyGpsInterval, 30.0);
        GpsWifiSwitch.IsToggled        = Preferences.Get(KeyGpsWifiStop, false);
        ScreenEventSwitch.IsToggled    = Preferences.Get(KeyScreenEvent, false);
        SmsForwardSwitch.IsToggled     = Preferences.Get(KeySmsForward, false);
        WidgetAutoRefreshSwitch.IsToggled = Preferences.Get(KeyWidgetAutoRefresh, false);
        WidgetRefreshIntervalSlider.Value = Preferences.Get(KeyWidgetRefreshMins, 15.0);
        ScreenEventGlobalSwitch.IsToggled = Preferences.Get(KeyScreenEvent, false);
        WifiStopGpsSwitch.IsToggled    = Preferences.Get(KeyGpsWifiStop, false);
        KeyControlSwitch.IsToggled     = Preferences.Get(KeyKeyControl, false);
        LanAutoSwitchCard.IsToggled    = Preferences.Get(KeyLanAutoSwitch, false);
    }

    // ─────────────────────────────────────────────
    // GPS handlers
    // ─────────────────────────────────────────────

    private async void OnGpsAuthClicked(object? sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        GpsGranted = status == PermissionStatus.Granted;
        GpsPermissionStatus = GpsGranted ? "已授权" : "未授权";
    }

    private void OnGpsIntervalChanged(object? sender, ValueChangedEventArgs e)
        => Preferences.Set(KeyGpsInterval, e.NewValue);

    private void OnGpsWifiSwitchToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set(KeyGpsWifiStop, e.Value);
        WifiStopGpsSwitch.IsToggled = e.Value;   // keep in sync with group 5
    }

    // ─────────────────────────────────────────────
    // Notify handlers
    // ─────────────────────────────────────────────

    private async void OnNotifyAppsTapped(object? sender, TappedEventArgs e)
    {
        var page = new AppSelectPage
        {
            SelectionConfirmed = _ => RefreshNotifyAppCount()
        };
        await Navigation.PushAsync(page);
    }

    private void OnOpenNotifyAccessSettingsClicked(object? sender, EventArgs e)
        => AndroidPermissionHelper.OpenNotificationListenerSettings();

    private void OnScreenEventSwitchToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set(KeyScreenEvent, e.Value);
        ScreenEventGlobalSwitch.IsToggled = e.Value;  // keep in sync with group 5
    }

    // ─────────────────────────────────────────────
    // SMS handlers
    // ─────────────────────────────────────────────

    private async void OnSmsAuthClicked(object? sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.Sms>();
        SmsGranted = status == PermissionStatus.Granted;
        SmsPermissionStatus = SmsGranted ? "已授权" : "未授权";
    }

    private void OnSmsForwardSwitchToggled(object? sender, ToggledEventArgs e)
        => Preferences.Set(KeySmsForward, e.Value);

    // ─────────────────────────────────────────────
    // Widget handlers
    // ─────────────────────────────────────────────

    private void OnManageWidgetsClicked(object? sender, EventArgs e)
    {
        // TODO: navigate to widget management page
    }

    private void OnWidgetAutoRefreshToggled(object? sender, ToggledEventArgs e)
        => Preferences.Set(KeyWidgetAutoRefresh, e.Value);

    private void OnWidgetRefreshIntervalChanged(object? sender, ValueChangedEventArgs e)
        => Preferences.Set(KeyWidgetRefreshMins, e.NewValue);

    // ─────────────────────────────────────────────
    // Feature toggle handlers (Group 5)
    // ─────────────────────────────────────────────

    private void OnScreenEventGlobalToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set(KeyScreenEvent, e.Value);
        ScreenEventSwitch.IsToggled = e.Value;   // keep in sync with group 2
    }

    private void OnWifiStopGpsToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Set(KeyGpsWifiStop, e.Value);
        GpsWifiSwitch.IsToggled = e.Value;       // keep in sync with group 1
    }

    private void OnKeyControlToggled(object? sender, ToggledEventArgs e)
        => Preferences.Set(KeyKeyControl, e.Value);

    private void OnLanAutoSwitchToggled(object? sender, ToggledEventArgs e)
        => Preferences.Set(KeyLanAutoSwitch, e.Value);

    // ─────────────────────────────────────────────
    // System permission tap (generic)
    // ─────────────────────────────────────────────

    private void OnOpenSystemPermissionTapped(object? sender, TappedEventArgs e)
        => AndroidPermissionHelper.OpenSystemPermission(e.Parameter as string ?? string.Empty);

    private void OnOpenGpsSystemSettingsClicked(object? sender, EventArgs e)
        => AndroidPermissionHelper.OpenAppDetailsSettings();
}
