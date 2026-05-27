namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class Gauge : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Gauge), 0.0,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Gauge), 0.0,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Gauge), 100.0,
            propertyChanged: OnValueChanged);

    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(Gauge), "%");

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Gauge), string.Empty);

    public Gauge()
    {
        InitializeComponent();
        UpdateProgress();
        UpdateGaugeSize();
        SizeManager.SizeChanged += (s, e) => UpdateGaugeSize();
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
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
            gauge.UpdateProgress();
        }
    }

    private void UpdateDisplayValue()
    {
        ValueLabel.Text = Math.Round(Value, 1).ToString();
    }

    private void UpdateGaugeSize()
    {
        double frameSize;
        double innerSize;
        
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                frameSize = 120;
                innerSize = 112;
                ValueLabel.FontSize = 28;
                UnitLabel.FontSize = 12;
                LabelLabel.FontSize = 12;
                break;
            case ComponentSize.Tablet:
                frameSize = 168;
                innerSize = 158;
                ValueLabel.FontSize = 38;
                UnitLabel.FontSize = 16;
                LabelLabel.FontSize = 14;
                break;
            case ComponentSize.TV:
                frameSize = 216;
                innerSize = 204;
                ValueLabel.FontSize = 52;
                UnitLabel.FontSize = 20;
                LabelLabel.FontSize = 18;
                break;
            default:
                frameSize = 120;
                innerSize = 112;
                ValueLabel.FontSize = 28;
                UnitLabel.FontSize = 12;
                LabelLabel.FontSize = 12;
                break;
        }
        
        this.WidthRequest = frameSize;
        this.HeightRequest = frameSize + 30; // 加上标签高度
        
        GaugeFrame.WidthRequest = frameSize;
        GaugeFrame.HeightRequest = frameSize;
        GaugeFrame.CornerRadius = (float)(frameSize / 2);
        
        BackgroundBox.WidthRequest = innerSize;
        BackgroundBox.HeightRequest = innerSize;
        BackgroundBox.CornerRadius = innerSize / 2;
        
        ProgressBox.WidthRequest = innerSize;
        ProgressBox.HeightRequest = innerSize;
        ProgressBox.CornerRadius = innerSize / 2;
    }

    private void UpdateProgress()
    {
        double range = Maximum - Minimum;
        double percentage = range == 0 ? 0 : Math.Max(0, Math.Min(1, (Value - Minimum) / range));
        
        // Home Assistant 风格的颜色：绿色 -> 黄色 -> 红色
        if (percentage >= 0.9)
            ProgressBox.BackgroundColor = Color.FromHex("#EF4444");
        else if (percentage >= 0.7)
            ProgressBox.BackgroundColor = Color.FromHex("#F59E0B");
        else
            ProgressBox.BackgroundColor = Color.FromHex("#10B981");
        
        // 使用透明度表示进度
        ProgressBox.Opacity = 0.3 + (percentage * 0.7);
        
        // 更新显示值
        UpdateDisplayValue();
    }
}