using System.Collections.ObjectModel;
using System.Text.Json;
using HassWebView.AndroidService.Models;
using HassWebView.AndroidService.NotificationForwarding;

namespace HassWebView.DemoAndroid;

public partial class AppSelectPage : ContentPage
{
    private const string SelectedPackagesKey = "notify_selected_packages";

    // All installed apps (unfiltered)
    private List<InstalledAppInfo> _allApps = [];

    // Currently shown list (after search filter)
    public ObservableCollection<InstalledAppInfo> FilteredApps { get; } = [];

    // Callback invoked when the user confirms selection
    public Action<IReadOnlyList<InstalledAppInfo>>? SelectionConfirmed { get; set; }

    // ─────────────────────────────────────────────
    // Persistence helpers
    // ─────────────────────────────────────────────

    /// <summary>Returns the count of persisted selected packages.</summary>
    public static int GetSelectedCount()
    {
        var packages = LoadSelectedPackages();
        return packages.Count;
    }

    private static List<string> LoadSelectedPackages()
    {
        var json = Preferences.Get(SelectedPackagesKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }

    private static void SaveSelectedPackages(IEnumerable<string> packages)
    {
        var json = JsonSerializer.Serialize(packages.ToList());
        Preferences.Set(SelectedPackagesKey, json);
    }

    public AppSelectPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_allApps.Count == 0)
            await LoadAppsAsync();
    }

    // ─────────────────────────────────────────────
    // Load apps
    // ─────────────────────────────────────────────

    private async Task LoadAppsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        AppList.IsVisible = false;

        _allApps = await Task.Run(NotificationForwardingManager.GetLauncherApps);

        // Restore saved selections and sort selected items to top
        var savedPackages = LoadSelectedPackages().ToHashSet();
        foreach (var app in _allApps)
            app.IsSelected = savedPackages.Contains(app.PackageName);

        _allApps = [
            .. _allApps.Where(a => a.IsSelected).OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase),
            .. _allApps.Where(a => !a.IsSelected).OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
        ];

        ApplyFilter(SearchBar.Text);

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        AppList.IsVisible = true;

        UpdateSummary();
    }

    // ─────────────────────────────────────────────
    // Search / filter
    // ─────────────────────────────────────────────

    private void ApplyFilter(string? query)
    {
        FilteredApps.Clear();

        var keyword = query?.Trim() ?? string.Empty;
        var source = string.IsNullOrEmpty(keyword)
            ? _allApps
            : _allApps.Where(a =>
                a.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                a.PackageName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        foreach (var app in source)
            FilteredApps.Add(app);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter(e.NewTextValue);
        UpdateSummary();
    }

    // ─────────────────────────────────────────────
    // Selection state
    // ─────────────────────────────────────────────

    private void OnRowTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is InstalledAppInfo app)
        {
            app.IsSelected = !app.IsSelected;
            // Force CollectionView to refresh the row
            var idx = FilteredApps.IndexOf(app);
            if (idx >= 0)
            {
                FilteredApps.RemoveAt(idx);
                FilteredApps.Insert(idx, app);
            }
            UpdateSummary();
        }
    }

    private void OnCheckBoxCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        UpdateSummary();
    }

    private void OnClearAllClicked(object? sender, EventArgs e)
    {
        foreach (var app in _allApps)
            app.IsSelected = false;

        // Refresh visible list
        ApplyFilter(SearchBar.Text);
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var count = _allApps.Count(a => a.IsSelected);
        ConfirmButton.Text = $"确定 ({count})";
        SummaryLabel.Text = count > 0 ? $"已选 {count} 个" : string.Empty;
    }

    // ─────────────────────────────────────────────
    // Confirm / cancel
    // ─────────────────────────────────────────────

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        var selected = _allApps.Where(a => a.IsSelected).ToList();
        SaveSelectedPackages(selected.Select(a => a.PackageName));
        SelectionConfirmed?.Invoke(selected);
        // Shell navigation: go back to the previous page in stack if any,
        // otherwise just collapse the flyout (top-level ShellContent).
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
        else
            Shell.Current.FlyoutIsPresented = false;
    }
}
