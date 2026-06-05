namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class SliderCard : AdaptiveComponent
{
    public SliderCard()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SliderCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(SliderCard), 0.0,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty MinProperty =
        BindableProperty.Create(nameof(Min), typeof(double), typeof(SliderCard), 0.0);

    public static readonly BindableProperty MaxProperty =
        BindableProperty.Create(nameof(Max), typeof(double), typeof(SliderCard), 100.0);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(SliderCard), "%");

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly BindableProperty ValuePrecisionProperty =
        BindableProperty.Create(nameof(ValuePrecision), typeof(int), typeof(SliderCard), 0,
            propertyChanged: OnValueChanged);

    public int ValuePrecision
    {
        get => (int)GetValue(ValuePrecisionProperty);
        set => SetValue(ValuePrecisionProperty, value);
    }

    public string DisplayValue => FormatValue(Value, ValuePrecision);

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SliderCard card)
        {
            card.OnPropertyChanged(nameof(DisplayValue));
        }
    }

    private string FormatValue(double value, int precision)
    {
        if (precision <= 0)
        {
            return Math.Round(value).ToString();
        }
        return value.ToString($"F{precision}");
    }
}