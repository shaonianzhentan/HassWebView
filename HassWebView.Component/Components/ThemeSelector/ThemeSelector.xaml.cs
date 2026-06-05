namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using HassWebView.Component.Models;
using Microsoft.Maui.Controls;
using System;

public partial class ThemeSelector : AdaptiveComponent
{
    public static readonly BindableProperty IsLightSelectedProperty = BindableProperty.Create(
        nameof(IsLightSelected), typeof(bool), typeof(ThemeSelector), false);

    public static readonly BindableProperty IsDarkSelectedProperty = BindableProperty.Create(
        nameof(IsDarkSelected), typeof(bool), typeof(ThemeSelector), false);

    public static readonly BindableProperty IsSystemSelectedProperty = BindableProperty.Create(
        nameof(IsSystemSelected), typeof(bool), typeof(ThemeSelector), false);

    public bool IsLightSelected
    {
        get => (bool)GetValue(IsLightSelectedProperty);
        set => SetValue(IsLightSelectedProperty, value);
    }

    public bool IsDarkSelected
    {
        get => (bool)GetValue(IsDarkSelectedProperty);
        set => SetValue(IsDarkSelectedProperty, value);
    }

    public bool IsSystemSelected
    {
        get => (bool)GetValue(IsSystemSelectedProperty);
        set => SetValue(IsSystemSelectedProperty, value);
    }

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
            var lightSelected = ThemeManager.CurrentTheme == ThemeMode.Light;
            var darkSelected = ThemeManager.CurrentTheme == ThemeMode.Dark;
            var systemSelected = ThemeManager.CurrentTheme == ThemeMode.System;
            
            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates: Light={lightSelected}, Dark={darkSelected}, System={systemSelected}");
            
            // 更新按钮状态
            IsLightSelected = lightSelected;
            IsDarkSelected = darkSelected;
            IsSystemSelected = systemSelected;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates failed: {ex}");
        }
    }
}