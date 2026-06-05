namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class SettingsGroup : AdaptiveComponent
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingsGroup), string.Empty);

    public SettingsGroup()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}