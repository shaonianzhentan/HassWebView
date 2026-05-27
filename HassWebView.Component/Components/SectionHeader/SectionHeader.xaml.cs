namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SectionHeader : ContentView
{
    public SectionHeader()
    {
        InitializeComponent();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
    }

    public event EventHandler? ActionTapped;

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SectionHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ActionProperty =
        BindableProperty.Create(nameof(Action), typeof(string), typeof(SectionHeader), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is SectionHeader header)
                    header.ActionLabel.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    public string? Action
    {
        get => (string?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                TitleLabel.FontSize = 12;
                ActionLabel.FontSize = 12;
                break;
            case ComponentSize.Tablet:
                TitleLabel.FontSize = 18;
                ActionLabel.FontSize = 18;
                break;
            case ComponentSize.TV:
                TitleLabel.FontSize = 24;
                ActionLabel.FontSize = 24;
                break;
        }
    }

    private void OnActionTapped(object sender, TappedEventArgs e)
    {
        ActionTapped?.Invoke(this, EventArgs.Empty);
    }
}