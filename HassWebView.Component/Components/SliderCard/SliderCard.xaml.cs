namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SliderCard : ContentView
{
    public SliderCard()
    {
        InitializeComponent();
        UpdateSize();
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

    private void UpdateSize()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            if (resources.TryGetValue("ComponentPaddingMedium", out var padMed) && padMed is Thickness padMedVal)
                CardBorder.Padding = padMedVal;
            
            if (resources.TryGetValue("ComponentTitleSizeMedium", out var titleMed) && titleMed is double titleMedVal)
            {
                TitleLabel.FontSize = titleMedVal;
                ValueLabel.FontSize = titleMedVal;
            }
            
            ValueSlider.HeightRequest = SizeManager.CurrentSize switch
            {
                Models.ComponentSize.Tablet => 56,
                Models.ComponentSize.TV => 72,
                _ => 40
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SliderCard.UpdateSize failed: {ex}");
        }
    }
}