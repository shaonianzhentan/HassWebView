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
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            InputEntry.Focused -= OnFocused;
            InputEntry.Unfocused -= OnUnfocused;
        }
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
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取字体大小
            if (resources.TryGetValue("ComponentBodySizeMedium", out var bodyMed) && bodyMed is double bodyMedVal)
                LabelLabel.FontSize = bodyMedVal;
            
            if (resources.TryGetValue("ComponentTitleSizeLarge", out var titleLg) && titleLg is double titleLgVal)
                InputEntry.FontSize = titleLgVal;
            
            if (resources.TryGetValue("ComponentBodySizeSmall", out var bodySmall) && bodySmall is double bodySmallVal)
                ErrorLabel.FontSize = bodySmallVal;
            
            // Padding
            if (resources.TryGetValue("ComponentPaddingMedium", out var padMed) && padMed is Thickness padMedVal)
                InputBorder.Padding = padMedVal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Input.UpdateInputSize failed: {ex}");
        }
    }

    private void OnFocused(object? sender, FocusEventArgs e)
    {
        // 使用主题颜色表示聚焦状态
        InputBorder.Stroke = new SolidColorBrush(Color.FromArgb("#007AFF"));
    }

    private void OnUnfocused(object? sender, FocusEventArgs e)
    {
        // 恢复默认边框颜色
        InputBorder.Stroke = new SolidColorBrush(Color.FromArgb("#E5E5EA"));
    }
}