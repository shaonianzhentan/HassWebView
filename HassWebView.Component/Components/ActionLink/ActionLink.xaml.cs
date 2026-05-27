namespace HassWebView.Component.Components;

public partial class ActionLink : ContentView
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

    public event EventHandler Clicked;
}