namespace HassWebView.Component.Models;

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public static class ThemeManager
{
    public static ThemeMode CurrentTheme { get; set; } = ThemeMode.System;
    
    public static event EventHandler? ThemeChanged;
    
    public static void SetTheme(ThemeMode theme)
    {
        CurrentTheme = theme;
        
        // 应用主题到应用程序
        if (Application.Current != null)
        {
            switch (theme)
            {
                case ThemeMode.Light:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
                case ThemeMode.Dark:
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    break;
                case ThemeMode.System:
                    Application.Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
        }
        
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }
}