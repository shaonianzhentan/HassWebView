namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class InfoRow : ContentView
{
    public InfoRow()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(InfoRow), string.Empty);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(InfoRow), string.Empty);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(InfoRow), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}