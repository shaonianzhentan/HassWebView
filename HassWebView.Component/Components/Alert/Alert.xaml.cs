namespace HassWebView.Component.Components;

public partial class Alert : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Alert), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(Alert), string.Empty);

    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(AlertType), typeof(Alert), AlertType.Warning);

    public Alert()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public AlertType Type
    {
        get => (AlertType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
    }
}

public enum AlertType
{
    Warning,
    Error,
    Success,
    Info
}