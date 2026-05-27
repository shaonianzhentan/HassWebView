namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class Input : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Input), string.Empty);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Input), string.Empty);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Input), string.Empty);

    public static readonly BindableProperty ShowLabelProperty =
        BindableProperty.Create(nameof(ShowLabel), typeof(bool), typeof(Input), true);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Input), false);

    public static readonly BindableProperty KeyboardTypeProperty =
        BindableProperty.Create(nameof(KeyboardType), typeof(Keyboard), typeof(Input), Keyboard.Default);

    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(Input), string.Empty);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Input), false);

    public Input()
    {
        InitializeComponent();
        InputEntry.Focused += OnFocused;
        InputEntry.Unfocused += OnUnfocused;
        UpdateInputSize();
        SizeManager.SizeChanged += (s, e) => UpdateInputSize();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public Keyboard KeyboardType
    {
        get => (Keyboard)GetValue(KeyboardTypeProperty);
        set => SetValue(KeyboardTypeProperty, value);
    }

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    private void UpdateInputSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                LabelLabel.FontSize = 13;
                InputEntry.FontSize = 16;
                ErrorLabel.FontSize = 12;
                InputBorder.Padding = new Thickness(12);
                break;
            case ComponentSize.Tablet:
                LabelLabel.FontSize = 16;
                InputEntry.FontSize = 20;
                ErrorLabel.FontSize = 14;
                InputBorder.Padding = new Thickness(16);
                break;
            case ComponentSize.TV:
                LabelLabel.FontSize = 20;
                InputEntry.FontSize = 26;
                ErrorLabel.FontSize = 18;
                InputBorder.Padding = new Thickness(20);
                break;
        }
    }

    private void OnFocused(object sender, FocusEventArgs e)
    {
        InputBorder.Stroke = Color.FromHex("#007AFF");
        InputBorder.BackgroundColor = Color.FromHex("#F0F7FF");
    }

    private void OnUnfocused(object sender, FocusEventArgs e)
    {
        InputBorder.Stroke = Color.FromHex("#E5E5EA");
        InputBorder.BackgroundColor = Colors.White;
    }
}