namespace HassWebView.Component.Models;

using Microsoft.Maui.Controls;

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public static class ThemeManager
{
    public static ThemeMode CurrentTheme { get; private set; } = ThemeMode.Dark;
    
    public static event EventHandler? ThemeChanged;
    
    private static bool _isInitialized = false;
    private static ResourceDictionary? _currentThemeDictionary;
    
    public static void Initialize()
    {
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] Initialize called. _isInitialized={_isInitialized}");
        
        if (_isInitialized)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeManager] Already initialized, skipping");
            return;
        }
            
        if (Application.Current?.Resources == null)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeManager] ERROR: Application.Current or Resources is null");
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] Application.Current.Resources found. MergedDictionaries count: {Application.Current.Resources.MergedDictionaries?.Count ?? 0}");
        
        _isInitialized = true;
        
        // 直接设置深色主题为默认
        SetTheme(ThemeMode.Dark);
        
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] Initialized successfully. Current theme: {CurrentTheme}");
    }
    
    public static void SetTheme(ThemeMode theme)
    {
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] SetTheme called: {theme}. CurrentTheme: {CurrentTheme}, _currentThemeDictionary: {_currentThemeDictionary != null}");
        
        if (CurrentTheme == theme && _currentThemeDictionary != null)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeManager] Same theme and dictionary exists, skipping");
            return;
        }
            
        CurrentTheme = theme;
        
        // 使用官方推荐的方式切换主题
        if (Application.Current?.Resources != null)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            
            if (mergedDictionaries != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] MergedDictionaries count before change: {mergedDictionaries.Count}");
                
                // 移除旧的主题字典
                if (_currentThemeDictionary != null && mergedDictionaries.Contains(_currentThemeDictionary))
                {
                    mergedDictionaries.Remove(_currentThemeDictionary);
                    System.Diagnostics.Debug.WriteLine("[ThemeManager] Removed old theme dictionary");
                }
                
                // 创建新的主题字典
                ResourceDictionary newThemeDict;
                switch (theme)
                {
                    case ThemeMode.Light:
                        newThemeDict = new Resources.Themes.LightTheme();
                        break;
                    case ThemeMode.Dark:
                        newThemeDict = new Resources.Themes.DarkTheme();
                        break;
                    case ThemeMode.System:
                        // 系统主题：根据当前系统主题选择
                        var useDarkTheme = Application.Current.RequestedTheme == AppTheme.Dark;
                        newThemeDict = useDarkTheme 
                            ? new Resources.Themes.DarkTheme() 
                            : new Resources.Themes.LightTheme();
                        break;
                    default:
                        newThemeDict = new Resources.Themes.LightTheme();
                        break;
                }
                
                // 添加新主题到集合中
                mergedDictionaries.Add(newThemeDict);
                _currentThemeDictionary = newThemeDict;
                
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Added new theme dictionary. MergedDictionaries count: {mergedDictionaries.Count}");
                
                // 验证资源是否存在
                if (Application.Current.Resources.TryGetValue("PageBackgroundColor", out var bgColor))
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] PageBackgroundColor resource found: {bgColor}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ThemeManager] WARNING: PageBackgroundColor resource NOT found!");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ThemeManager] ERROR: MergedDictionaries is null");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[ThemeManager] ERROR: Application.Current.Resources is null");
        }
        
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] Theme changed to: {theme}");
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }
}