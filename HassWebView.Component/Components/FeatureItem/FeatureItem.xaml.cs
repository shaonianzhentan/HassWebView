namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class FeatureItem : AdaptiveComponent
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FeatureItem), string.Empty);

    public FeatureItem()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}