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
            typeof(ComponentSize?),
            typeof(SizeableComponent),
            null);

    /// <summary>
    /// 组件尺寸，为 null 时使用全局配置
    /// </summary>
    public ComponentSize? Size
    {
        get => (ComponentSize?)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
