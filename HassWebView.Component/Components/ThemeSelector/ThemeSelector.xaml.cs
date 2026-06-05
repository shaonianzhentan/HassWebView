namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using HassWebView.Component.Models;
using Microsoft.Maui.Controls;
using System;

public partial class ThemeSelector : AdaptiveComponent
{
    public ThemeSelector()
    {
        InitializeComponent();
        
        // 监听组件加载事件
        Loaded += OnLoaded;
        
        // 监听组件卸载事件
        Unloaded += OnUnloaded;
    }
    
    private void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector loaded, CurrentTheme: {ThemeManager.CurrentTheme}");
            
            // 同步当前状态
            UpdateButtonStates();
            
            // 订阅主题变化事件
            ThemeManager.ThemeChanged += OnThemeChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnLoaded failed: {ex}");
        }
    }
    
    private void OnUnloaded(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector unloaded");
            
            // 取消订阅主题变化事件
            ThemeManager.ThemeChanged -= OnThemeChanged;
            
            // 取消订阅自身事件
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnUnloaded failed: {ex}");
        }
    }
    
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnThemeChanged: {ThemeManager.CurrentTheme}");
            UpdateButtonStates();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnThemeChanged failed: {ex}");
        }
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is string themeStr)
            {
                System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnThemeClicked: {themeStr}");
                
                if (Enum.TryParse<ThemeMode>(themeStr, out var theme))
                {
                    ThemeManager.SetTheme(theme);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeSelector.OnThemeClicked failed: {ex}");
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