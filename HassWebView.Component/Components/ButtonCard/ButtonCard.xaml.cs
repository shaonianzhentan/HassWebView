namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ButtonCard : ContentView
{
    public ButtonCard()
    {
        InitializeComponent();
        UpdateStateColor();
        UpdateSize();
    }

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(ButtonCard), string.Empty);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ButtonCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(ButtonCard), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is ButtonCard card)
                    card.UpdateStateColor();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(ButtonCard), false);

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly BindableProperty ShowBadgeProperty =
        BindableProperty.Create(nameof(ShowBadge), typeof(bool), typeof(ButtonCard), false);

    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(ButtonCard), Color.FromArgb("#636366"));

    private void UpdateStateColor()
    {
        Color color = State?.ToLower() switch
        {
            "on" => Color.FromArgb("#34C759"),
            "open" => Color.FromArgb("#34C759"),
            "locked" => Color.FromArgb("#34C759"),
            _ => Color.FromArgb("#636366")
        };
        StateColor = color;
        
        // 如果已经初始化，更新主题
        if (StateLabel != null)
        {
            StateLabel.TextColor = color;
        }
    }

    private void UpdateSize()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取尺寸值
            if (resources.TryGetValue("ComponentPaddingMedium", out var padMed) && padMed is Thickness padMedVal)
                CardBorder.Padding = padMedVal;
            
            if (resources.TryGetValue("ComponentIconSizeMedium", out var iconMed) && iconMed is double iconMedVal)
                IconLabel.FontSize = iconMedVal;
            
            if (resources.TryGetValue("ComponentTitleSizeMedium", out var titleMed) && titleMed is double titleMedVal)
                TitleLabel.FontSize = titleMedVal;
            
            if (resources.TryGetValue("ComponentBodySizeMedium", out var bodyMed) && bodyMed is double bodyMedVal)
                StateLabel.FontSize = bodyMedVal;
            
            // Badge padding 使用特定值
            if (SizeManager.CurrentSize == ComponentSize.Phone)
                BadgeBorder.Padding = new Thickness(6, 3);
            else if (SizeManager.CurrentSize == ComponentSize.Tablet)
                BadgeBorder.Padding = new Thickness(8, 4);
            else
                BadgeBorder.Padding = new Thickness(12, 6);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ButtonCard] UpdateSize failed: {ex.Message}");
        }
    }
}