namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class SectionHeader : AdaptiveComponent
{
    public SectionHeader()
    {
        InitializeComponent();
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

    private void OnActionTapped(object sender, TappedEventArgs e)
    {
        ActionTapped?.Invoke(this, EventArgs.Empty);
    }
}