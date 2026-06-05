using HassWebView.Component.Models;

namespace HassWebView.Component.Components.Base;

/// <summary>
/// 支持尺寸配置的组件基类
/// </summary>
public class SizeableComponent : ContentView
{
    /// <summary>
    /// 定义 Size 绑定属性
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(
            nameof(Size),
            typeof(ComponentSize),
            typeof(SizeableComponent),
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

    public SizeableComponent()
    {
        // 初始化为全局尺寸
        UpdateEffectiveSize();
        
        // 监听全局尺寸变化
        SizeManager.SizeChanged += OnGlobalSizeChanged;
    }

    private static void OnSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SizeableComponent component)
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
