namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class DetailCard : AdaptiveComponent
{
    public DetailCard()
    {
        InitializeComponent();
        UpdateStateColorType();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(DetailCard), string.Empty);

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(DetailCard), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is DetailCard card)
                    card.UpdateStateColorType();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty StateColorTypeProperty =
        BindableProperty.Create(nameof(StateColorType), typeof(StateColorType), typeof(DetailCard), StateColorType.Success);

    public StateColorType StateColorType
    {
        get => (StateColorType)GetValue(StateColorTypeProperty);
        set => SetValue(StateColorTypeProperty, value);
    }

    private void UpdateStateColorType()
    {
        try
        {
            StateColorType colorType = State?.ToLower() switch
            {
                "on" => StateColorType.Success,
                "open" => StateColorType.Success,
                "off" => StateColorType.Default,
                "closed" => StateColorType.Default,
                "unavailable" => StateColorType.Error,
                _ => StateColorType.Success
            };
            StateColorType = colorType;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DetailCard.UpdateStateColorType failed: {ex}");
        }
    }
}