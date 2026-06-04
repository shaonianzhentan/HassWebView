namespace HassWebView.Component.Components;

public partial class ButtonCard : ContentView
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
                    card.UpdateStateColor();
            });

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(ButtonCard), false);

    public static readonly BindableProperty ShowBadgeProperty =
        BindableProperty.Create(nameof(ShowBadge), typeof(bool), typeof(ButtonCard), false);

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(ButtonCard), null,
            propertyChanged: (b, _, __) =>
            {
                if (b is ButtonCard card)
                    card.UpdateStateLabelColor();
            });

    public static readonly BindableProperty StateColorTypeProperty =
        BindableProperty.Create(nameof(StateColorType), typeof(StateColorType), typeof(ButtonCard), StateColorType.Default,
            propertyChanged: (b, _, __) =>
            {
                if (b is ButtonCard card)
                    card.ApplyStateColorType();
            });

    public ButtonCard()
    {
        InitializeComponent();
        UpdateStateColor();
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

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public StateColorType StateColorType
    {
        get => (StateColorType)GetValue(StateColorTypeProperty);
        set => SetValue(StateColorTypeProperty, value);
    }

    private void UpdateStateColor()
    {
        if (StateColor != null)
        {
            UpdateStateLabelColor();
            return;
        }

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

    private void ApplyStateColorType()
    {
        if (StateColor != null)
            return;

        var resources = Application.Current?.Resources;
        if (resources == null) return;

        string resourceKey = StateColorType switch
        {
            StateColorType.Success => "SuccessColor",
            StateColorType.Warning => "WarningColor",
            StateColorType.Error => "ErrorColor",
            _ => "SecondaryTextColor"
        };

        if (resources.TryGetValue(resourceKey, out var value) && value is Color color)
        {
            StateColor = color;
        }
    }

    private void UpdateStateLabelColor()
    {
        if (StateLabel != null && StateColor != null)
        {
            StateLabel.TextColor = StateColor;
        }
    }
}