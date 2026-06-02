namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ThemeSelector : ContentView
{
    public ThemeSelector()
    {
        InitializeComponent();
        // 延迟初始化以避免崩溃
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            UpdateButtonStates();
            ThemeManager.ThemeChanged += (s, e) => UpdateButtonStates();
        });
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string themeStr)
        {
            if (Enum.TryParse<ThemeMode>(themeStr, out var theme))
            {
                ThemeManager.SetTheme(theme);
            }
        }
    }

    private void UpdateButtonStates()
    {
        try
        {
            UpdateButtonStyle(LightBtn, ThemeManager.CurrentTheme == ThemeMode.Light);
            UpdateButtonStyle(DarkBtn, ThemeManager.CurrentTheme == ThemeMode.Dark);
            UpdateButtonStyle(SystemBtn, ThemeManager.CurrentTheme == ThemeMode.System);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates failed: {ex}");
        }
    }

    private void UpdateButtonStyle(Button button, bool isSelected)
    {
        if (button == null) return;
        
        if (isSelected)
        {
            button.BackgroundColor = Color.FromArgb("#007AFF");
            button.TextColor = Colors.White;
        }
        else
        {
            button.BackgroundColor = Color.FromArgb("#F2F2F7");
            button.TextColor = Color.FromArgb("#636366");
        }
    }
}