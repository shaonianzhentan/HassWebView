namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ThemeSelector : ContentView
{
    public ThemeSelector()
    {
        InitializeComponent();
        UpdateButtonStates();
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string themeStr)
        {
            if (Enum.TryParse<ThemeMode>(themeStr, out var theme))
            {
                ThemeManager.SetTheme(theme);
                UpdateButtonStates();
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
        
        // 使用 DynamicResource 获取主题颜色
        var accentColor = Application.Current?.Resources.TryGetValue("AccentColor", out var accent) == true 
            ? accent as Color 
            : Color.FromArgb("#007AFF");
        var cardBackgroundColor = Application.Current?.Resources.TryGetValue("CardBackgroundColor", out var cardBg) == true 
            ? cardBg as Color 
            : Color.FromArgb("#EFEFF4");
        var secondaryTextColor = Application.Current?.Resources.TryGetValue("SecondaryTextColor", out var secondaryText) == true 
            ? secondaryText as Color 
            : Color.FromArgb("#636366");
        var buttonTextColor = Application.Current?.Resources.TryGetValue("ButtonTextColor", out var btnText) == true 
            ? btnText as Color 
            : Colors.White;
        
        if (isSelected)
        {
            button.BackgroundColor = accentColor;
            button.TextColor = buttonTextColor;
        }
        else
        {
            button.BackgroundColor = cardBackgroundColor;
            button.TextColor = secondaryTextColor;
        }
    }
}