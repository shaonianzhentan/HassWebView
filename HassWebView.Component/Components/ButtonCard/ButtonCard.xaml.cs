namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ButtonCard : ContentView
{
    public ButtonCard()
    {
        InitializeComponent();
        UpdateStateColor();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
    }

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(ButtonCard), string.Empty);

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
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
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(ButtonCard), Color.FromHex("#636366"));

    private void UpdateStateColor()
    {
        Color color = State?.ToLower() switch
        {
            "on" => Color.FromHex("#34C759"),
            "open" => Color.FromHex("#34C759"),
            "locked" => Color.FromHex("#34C759"),
            _ => Color.FromHex("#636366")
        };
        StateColor = color;
    }

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                CardBorder.Padding = new Thickness(16);
                IconLabel.FontSize = 36;
                TitleLabel.FontSize = 14;
                StateLabel.FontSize = 12;
                BadgeBorder.Padding = new Thickness(6, 3);
                break;
            case ComponentSize.Tablet:
                CardBorder.Padding = new Thickness(20);
                IconLabel.FontSize = 48;
                TitleLabel.FontSize = 16;
                StateLabel.FontSize = 14;
                BadgeBorder.Padding = new Thickness(8, 4);
                break;
            case ComponentSize.TV:
                CardBorder.Padding = new Thickness(28);
                IconLabel.FontSize = 64;
                TitleLabel.FontSize = 22;
                StateLabel.FontSize = 18;
                BadgeBorder.Padding = new Thickness(12, 6);
                break;
        }
    }
}