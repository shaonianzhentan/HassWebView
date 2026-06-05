namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class Gauge : SizeableComponent
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Gauge), 0.0,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(Gauge), "%");

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Gauge), string.Empty);

    public Gauge()
    {
        InitializeComponent();
        UpdateDisplayValue();
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Gauge gauge)
        {
            gauge.UpdateDisplayValue();
        }
    }

    private void UpdateDisplayValue()
    {
        ValueLabel.Text = $"{Math.Round(Value, 1)}{Unit}";
    }
}