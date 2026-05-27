namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SliderCard : ContentView
{
    public SliderCard()
    {
        InitializeComponent();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SliderCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(SliderCard), 0.0);

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

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                CardBorder.Padding = new Thickness(16);
                TitleLabel.FontSize = 14;
                ValueLabel.FontSize = 14;
                ValueSlider.HeightRequest = 40;
                break;
            case ComponentSize.Tablet:
                CardBorder.Padding = new Thickness(24);
                TitleLabel.FontSize = 20;
                ValueLabel.FontSize = 20;
                ValueSlider.HeightRequest = 56;
                break;
            case ComponentSize.TV:
                CardBorder.Padding = new Thickness(32);
                TitleLabel.FontSize = 28;
                ValueLabel.FontSize = 28;
                ValueSlider.HeightRequest = 72;
                break;
        }
    }
}
