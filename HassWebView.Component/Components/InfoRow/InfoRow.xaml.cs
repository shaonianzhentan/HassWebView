namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class InfoRow : ContentView
{
    public InfoRow()
    {
        InitializeComponent();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
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

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                IconLabel.FontSize = 20;
                LabelLabel.FontSize = 14;
                ValueLabel.FontSize = 14;
                break;
            case ComponentSize.Tablet:
                IconLabel.FontSize = 28;
                LabelLabel.FontSize = 18;
                ValueLabel.FontSize = 18;
                break;
            case ComponentSize.TV:
                IconLabel.FontSize = 36;
                LabelLabel.FontSize = 24;
                ValueLabel.FontSize = 24;
                break;
        }
    }
}