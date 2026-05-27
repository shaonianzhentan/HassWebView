namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ThemeSelector : ContentView
{
    public ThemeSelector()
    {
        InitializeComponent();
        UpdateButtonStates();
        ThemeManager.ThemeChanged += (s, e) => UpdateButtonStates();
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
        UpdateButton(LightBtn, ThemeMode.Light);
        UpdateButton(DarkBtn, ThemeMode.Dark);
        UpdateButton(SystemBtn, ThemeMode.System);
    }

    private void UpdateButton(Button btn, ThemeMode theme)
    {
        bool isSelected = ThemeManager.CurrentTheme == theme;
        if (isSelected)
        {
            btn.BackgroundColor = Color.FromHex("#007AFF");
            btn.TextColor = Colors.White;
        }
        else
        {
            // 根据当前系统主题设置未选中按钮的颜色
            var isDark = Application.Current?.UserAppTheme == AppTheme.Dark;
            btn.BackgroundColor = isDark ? Color.FromHex("#3A3A3C") : Color.FromHex("#EFEFF4");
            btn.TextColor = isDark ? Colors.White : Color.FromHex("#1D1D1F");
        }
    }
}