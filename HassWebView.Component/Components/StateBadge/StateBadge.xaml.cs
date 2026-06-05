namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class StateBadge : SizeableComponent
{
    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(StateBadge), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is StateBadge badge)
                    badge.UpdateStateColor();
            });

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(StateColorType), typeof(StateBadge), StateColorType.Default);

    public StateBadge()
    {
        InitializeComponent();
        UpdateStateColor();
    }

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public StateColorType StateColor
    {
        get => (StateColorType)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    private void UpdateStateColor()
    {
        StateColorType colorType = State?.ToLower() switch
        {
            "on" => StateColorType.Success,
            "open" => StateColorType.Success,
            "active" => StateColorType.Success,
            "locked" => StateColorType.Success,
            "off" => StateColorType.Default,
            "closed" => StateColorType.Default,
            "inactive" => StateColorType.Default,
            "unlocked" => StateColorType.Default,
            "unavailable" => StateColorType.Error,
            "unknown" => StateColorType.Warning,
            _ => StateColorType.Default
        };
        StateColor = colorType;
    }
}

public enum StateColorType
{
    Default,
    Success,
    Warning,
    Error
}