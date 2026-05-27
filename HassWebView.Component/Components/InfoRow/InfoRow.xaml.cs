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

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(InfoRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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
                TitleLabel.FontSize = 14;
                ValueLabel.FontSize = 14;
                break;
            case ComponentSize.Tablet:
                TitleLabel.FontSize = 18;
                ValueLabel.FontSize = 18;
                break;
            case ComponentSize.TV:
                TitleLabel.FontSize = 24;
                ValueLabel.FontSize = 24;
                break;
        }
    }
}