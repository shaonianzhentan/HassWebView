namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using HassWebView.Component.Models;
using Microsoft.Maui.Controls;
using System;

public partial class SizeSelector : AdaptiveComponent
{
    private bool _isUpdatingFromBinding = false;
    
    public static readonly BindableProperty IsPhoneSelectedProperty = BindableProperty.Create(
        nameof(IsPhoneSelected), typeof(bool), typeof(SizeSelector), false,
        propertyChanged: OnIsPhoneSelectedChanged);

    public static readonly BindableProperty IsTabletSelectedProperty = BindableProperty.Create(
        nameof(IsTabletSelected), typeof(bool), typeof(SizeSelector), false,
        propertyChanged: OnIsTabletSelectedChanged);

    public static readonly BindableProperty IsTVSelectedProperty = BindableProperty.Create(
        nameof(IsTVSelected), typeof(bool), typeof(SizeSelector), false,
        propertyChanged: OnIsTVSelectedChanged);

    public bool IsPhoneSelected
    {
        get => (bool)GetValue(IsPhoneSelectedProperty);
        set => SetValue(IsPhoneSelectedProperty, value);
    }

    public bool IsTabletSelected
    {
        get => (bool)GetValue(IsTabletSelectedProperty);
        set => SetValue(IsTabletSelectedProperty, value);
    }

    public bool IsTVSelected
    {
        get => (bool)GetValue(IsTVSelectedProperty);
        set => SetValue(IsTVSelectedProperty, value);
    }

    public SizeSelector()
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
            System.Diagnostics.Debug.WriteLine($"SizeSelector loaded, CurrentSize: {SizeManager.CurrentSize}");
            
            // 同步当前状态
            UpdateButtonStates();
            
            // 订阅全局尺寸变化事件
            SizeManager.SizeChanged += OnSizeChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnLoaded failed: {ex}");
        }
    }
    
    private void OnUnloaded(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector unloaded");
            
            // 取消订阅全局尺寸变化事件
            SizeManager.SizeChanged -= OnSizeChanged;
            
            // 取消订阅自身事件
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnUnloaded failed: {ex}");
        }
    }
    
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnSizeChanged: {SizeManager.CurrentSize}");
            UpdateButtonStates();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnSizeChanged failed: {ex}");
        }
    }

    private static void OnIsPhoneSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SizeSelector selector && (bool)newValue)
        {
            System.Diagnostics.Debug.WriteLine($"IsPhoneSelected changed to true");
            selector._isUpdatingFromBinding = true;
            SizeManager.SetSize(ComponentSize.Phone);
            selector._isUpdatingFromBinding = false;
        }
    }

    private static void OnIsTabletSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SizeSelector selector && (bool)newValue)
        {
            System.Diagnostics.Debug.WriteLine($"IsTabletSelected changed to true");
            selector._isUpdatingFromBinding = true;
            SizeManager.SetSize(ComponentSize.Tablet);
            selector._isUpdatingFromBinding = false;
        }
    }

    private static void OnIsTVSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SizeSelector selector && (bool)newValue)
        {
            System.Diagnostics.Debug.WriteLine($"IsTVSelected changed to true");
            selector._isUpdatingFromBinding = true;
            SizeManager.SetSize(ComponentSize.TV);
            selector._isUpdatingFromBinding = false;
        }
    }

    private void OnSizeClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is string sizeStr)
            {
                System.Diagnostics.Debug.WriteLine($"SizeSelector.OnSizeClicked: {sizeStr}");
                
                if (Enum.TryParse<ComponentSize>(sizeStr, out var size))
                {
                    SizeManager.SetSize(size);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnSizeClicked failed: {ex}");
        }
    }

    private void UpdateButtonStates()
    {
        try
        {
            // 如果正在从绑定更新，跳过此方法以防止循环
            if (_isUpdatingFromBinding)
            {
                System.Diagnostics.Debug.WriteLine("UpdateButtonStates skipped due to _isUpdatingFromBinding");
                return;
            }
            
            var phoneSelected = SizeManager.CurrentSize == ComponentSize.Phone;
            var tabletSelected = SizeManager.CurrentSize == ComponentSize.Tablet;
            var tvSelected = SizeManager.CurrentSize == ComponentSize.TV;
            
            System.Diagnostics.Debug.WriteLine($"UpdateButtonStates: Phone={phoneSelected}, Tablet={tabletSelected}, TV={tvSelected}");
            
            // 更新按钮状态
            IsPhoneSelected = phoneSelected;
            IsTabletSelected = tabletSelected;
            IsTVSelected = tvSelected;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.UpdateButtonStates failed: {ex}");
        }
    }
}