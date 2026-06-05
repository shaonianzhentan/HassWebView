namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using HassWebView.Component.Models;

public partial class SizeSelector : SizeableComponent
{
    public static readonly BindableProperty IsPhoneSelectedProperty = BindableProperty.Create(
        nameof(IsPhoneSelected), typeof(bool), typeof(SizeSelector), false);

    public static readonly BindableProperty IsTabletSelectedProperty = BindableProperty.Create(
        nameof(IsTabletSelected), typeof(bool), typeof(SizeSelector), false);

    public static readonly BindableProperty IsTVSelectedProperty = BindableProperty.Create(
        nameof(IsTVSelected), typeof(bool), typeof(SizeSelector), false);

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
        
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            UpdateButtonStates();
        });
        
        // 监听尺寸变化事件
        SizeManager.SizeChanged += OnSizeChanged;
        
        // 监听组件卸载事件
        Unloaded += OnUnloaded;
    }
    
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }
    
    private void OnUnloaded(object? sender, EventArgs e)
    {
        SizeManager.SizeChanged -= OnSizeChanged;
        Unloaded -= OnUnloaded;
    }

    private void OnSizeClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is string sizeStr)
            {
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
            var phoneSelected = SizeManager.CurrentSize == ComponentSize.Phone;
            var tabletSelected = SizeManager.CurrentSize == ComponentSize.Tablet;
            var tvSelected = SizeManager.CurrentSize == ComponentSize.TV;
            
            // 手动触发属性变更通知
            if (IsPhoneSelected != phoneSelected)
            {
                IsPhoneSelected = phoneSelected;
            }
            if (IsTabletSelected != tabletSelected)
            {
                IsTabletSelected = tabletSelected;
            }
            if (IsTVSelected != tvSelected)
            {
                IsTVSelected = tvSelected;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.UpdateButtonStates failed: {ex}");
        }
    }
}