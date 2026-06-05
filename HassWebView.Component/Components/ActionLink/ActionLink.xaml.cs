namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class ActionLink : SizeableComponent
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ActionLink), string.Empty);

    public ActionLink()
    {
        InitializeComponent();
        LinkButton.Clicked += (s, e) => Clicked?.Invoke(this, e);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler? Clicked;
}