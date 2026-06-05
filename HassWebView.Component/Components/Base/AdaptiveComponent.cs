using HassWebView.Component.Models;

namespace HassWebView.Component.Components.Base;

/// <summary>
/// 自适应组件基类，支持根据屏幕尺寸自动调整
/// </summary>
public class AdaptiveComponent : ContentView
{
    #region Size BindableProperty

    /// <summary>
    /// 定义 Size 绑定属性
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(
            nameof(Size),
            typeof(ComponentSize),
            typeof(AdaptiveComponent),
            ComponentSize.Phone,
            propertyChanged: OnSizePropertyChanged);

    private ComponentSize _effectiveSize = SizeManager.CurrentSize;

    /// <summary>
    /// 组件尺寸，默认为全局配置
    /// </summary>
    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 获取实际生效的尺寸，优先使用组件自身尺寸，否则使用全局尺寸
    /// </summary>
    public ComponentSize EffectiveSize
    {
        get => _effectiveSize;
        private set
        {
            if (_effectiveSize != value)
            {
                _effectiveSize = value;
                OnPropertyChanged(nameof(EffectiveSize));
            }
        }
    }

    #endregion

    #region IsVisible BindableProperty

    /// <summary>
    /// 定义 IsVisible 绑定属性
    /// </summary>
    public static new readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(
            nameof(IsVisible),
            typeof(bool),
            typeof(AdaptiveComponent),
            true,
            propertyChanged: OnIsVisiblePropertyChanged);

    /// <summary>
    /// 组件是否可见
    /// </summary>
    public new bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    private static void OnIsVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveComponent component)
        {
            component.UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        base.IsVisible = IsVisible;
    }

    #endregion

    #region Opacity BindableProperty

    /// <summary>
    /// 定义 Opacity 绑定属性
    /// </summary>
    public static new readonly BindableProperty OpacityProperty =
        BindableProperty.Create(
            nameof(Opacity),
            typeof(double),
            typeof(AdaptiveComponent),
            1.0,
            propertyChanged: OnOpacityPropertyChanged);

    /// <summary>
    /// 组件透明度（0.0 - 1.0）
    /// </summary>
    public new double Opacity
    {
        get => (double)GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    private static void OnOpacityPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveComponent component)
        {
            component.UpdateOpacity();
        }
    }

    private void UpdateOpacity()
    {
        base.Opacity = Opacity;
    }

    #endregion

    #region IsEnabled BindableProperty

    /// <summary>
    /// 定义 IsEnabled 绑定属性
    /// </summary>
    public static new readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(
            nameof(IsEnabled),
            typeof(bool),
            typeof(AdaptiveComponent),
            true,
            propertyChanged: OnIsEnabledPropertyChanged);

    /// <summary>
    /// 组件是否启用
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveComponent component)
        {
            component.UpdateEnabled();
        }
    }

    private void UpdateEnabled()
    {
        base.IsEnabled = IsEnabled;
    }

    #endregion

    public AdaptiveComponent()
    {
        // 初始化为全局尺寸
        UpdateEffectiveSize();
        
        // 初始化可见性
        UpdateVisibility();
        
        // 初始化透明度
        UpdateOpacity();
        
        // 初始化启用状态
        UpdateEnabled();
        
        // 监听全局尺寸变化
        SizeManager.SizeChanged += OnGlobalSizeChanged;
    }

    private static void OnSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdaptiveComponent component)
        {
            component.UpdateEffectiveSize();
        }
    }

    private void OnGlobalSizeChanged(object? sender, EventArgs e)
    {
        UpdateEffectiveSize();
    }

    private void UpdateEffectiveSize()
    {
        EffectiveSize = Size == ComponentSize.Phone 
            ? SizeManager.CurrentSize 
            : Size;
    }
}
