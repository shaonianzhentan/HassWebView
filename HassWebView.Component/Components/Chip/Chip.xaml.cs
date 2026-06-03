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
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取字体大小
            if (resources.TryGetValue("ComponentBodySizeMedium", out var bodyMed) && bodyMed is double bodyMedVal)
            {
                IconLabel.FontSize = bodyMedVal;
                TextLabel.FontSize = bodyMedVal;
            }
            
            if (resources.TryGetValue("ComponentBodySizeSmall", out var bodySmall) && bodySmall is double bodySmallVal)
                CloseBtn.FontSize = bodySmallVal;
            
            // Padding
            if (resources.TryGetValue("ComponentPaddingSmall", out var padSmall) && padSmall is Thickness padSmallVal)
                ChipBorder.Padding = padSmallVal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chip.UpdateChipSize failed: {ex}");
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
                ChipBorder.BackgroundColor = Color.FromArgb("#34C759");
                TextLabel.TextColor = Colors.White;
                break;
            case ChipType.Warning:
                ChipBorder.BackgroundColor = Color.FromArgb("#FF9500");
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