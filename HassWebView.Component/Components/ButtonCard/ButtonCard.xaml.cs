namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class ButtonCard : AdaptiveComponent
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(ButtonCard), string.Empty);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ButtonCard), string.Empty);

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(ButtonCard), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is ButtonCard card)
                    card.UpdateStateColorType();
            });

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(ButtonCard), false);

    public static readonly BindableProperty ShowBadgeProperty =
        BindableProperty.Create(nameof(ShowBadge), typeof(bool), typeof(ButtonCard), false);

    public static readonly BindableProperty StateColorTypeProperty =
        BindableProperty.Create(nameof(StateColorType), typeof(StateColorType), typeof(ButtonCard), StateColorType.Default);

    public ButtonCard()
    {
        InitializeComponent();
        UpdateStateColorType();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    public StateColorType StateColorType
    {
        get => (StateColorType)GetValue(StateColorTypeProperty);
        set => SetValue(StateColorTypeProperty, value);
    }

    private void UpdateStateColorType()
    {
        StateColorType colorType = State?.ToLower() switch
        {
            "on" => StateColorType.Success,
            "open" => StateColorType.Success,
            "locked" => StateColorType.Success,
            "active" => StateColorType.Success,
            "playing" => StateColorType.Success,
            "unavailable" => StateColorType.Error,
            _ => StateColorType.Default
        };
        StateColorType = colorType;
    }
}