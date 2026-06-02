namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

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
        UpdateChipStyle();
        UpdateChipSize();
        SizeManager.SizeChanged += (s, e) => UpdateChipSize();
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

    private void UpdateChipSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                IconLabel.FontSize = 14;
                TextLabel.FontSize = 14;
                CloseBtn.FontSize = 12;
                ChipBorder.Padding = new Thickness(10, 6);
                break;
            case ComponentSize.Tablet:
                IconLabel.FontSize = 18;
                TextLabel.FontSize = 18;
                CloseBtn.FontSize = 16;
                ChipBorder.Padding = new Thickness(14, 10);
                break;
            case ComponentSize.TV:
                IconLabel.FontSize = 24;
                TextLabel.FontSize = 24;
                CloseBtn.FontSize = 20;
                ChipBorder.Padding = new Thickness(18, 14);
                break;
        }
    }

    private void UpdateChipStyle()
    {
        switch (Type)
        {
            case ChipType.Default:
                ChipBorder.BackgroundColor = Color.FromArgb("#EFEFF4");
                TextLabel.TextColor = Color.FromArgb("#1D1D1F");
                break;
            case ChipType.Primary:
                ChipBorder.BackgroundColor = Color.FromArgb("#007AFF");
                TextLabel.TextColor = Colors.White;
                break;
            case ChipType.Secondary:
                ChipBorder.BackgroundColor = Color.FromArgb("#3C3C3C");
                TextLabel.TextColor = Colors.White;
                break;
            case ChipType.Success:
                ChipBorder.BackgroundColor = Color.FromArgb("#30D158");
                TextLabel.TextColor = Colors.White;
                break;
            case ChipType.Warning:
                ChipBorder.BackgroundColor = Color.FromArgb("#FF9F0A");
                TextLabel.TextColor = Colors.White;
                break;
            case ChipType.Error:
                ChipBorder.BackgroundColor = Color.FromArgb("#FF3B30");
                TextLabel.TextColor = Colors.White;
                break;
        }
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