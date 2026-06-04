namespace HassWebView.Component.Components;

public partial class Chip : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Chip), string.Empty);

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(Chip), string.Empty);

    public static readonly BindableProperty ShowIconProperty =
        BindableProperty.Create(nameof(ShowIcon), typeof(bool), typeof(Chip), true);

    public static readonly BindableProperty DismissibleProperty =
        BindableProperty.Create(nameof(Dismissible), typeof(bool), typeof(Chip), false);

    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(ChipType), typeof(Chip), ChipType.Default);

    public Chip()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public bool Dismissible
    {
        get => (bool)GetValue(DismissibleProperty);
        set => SetValue(DismissibleProperty, value);
    }

    public ChipType Type
    {
        get => (ChipType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
    }
}

public enum ChipType
{
    Default,
    Primary,
    Secondary,
    Success,
    Warning,
    Error
}