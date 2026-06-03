namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SliderCard : ContentView
{
    public SliderCard()
    {
        InitializeComponent();
        UpdateSize();
        SizeManager.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateSize());
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            SizeManager.SizeChanged -= OnSizeChanged;
        }
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
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取尺寸值
            if (resources.TryGetValue("ComponentPaddingMedium", out var padMed) && padMed is Thickness padMedVal)
                CardBorder.Padding = padMedVal;
            
            if (resources.TryGetValue("ComponentTitleSizeMedium", out var titleMed) && titleMed is double titleMedVal)
            {
                TitleLabel.FontSize = titleMedVal;
                ValueLabel.FontSize = titleMedVal;
            }
            
            // HeightRequest 根据尺寸设置
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
