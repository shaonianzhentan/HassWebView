namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class EntityRow : AdaptiveComponent
{
    public EntityRow()
    {
        InitializeComponent();
        UpdateStateColorType();
    }

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(EntityRow), string.Empty);

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(EntityRow), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.UpdateStateColorType();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(EntityRow), false,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.UpdateStateColorType();
            });

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly BindableProperty StateColorTypeProperty =
        BindableProperty.Create(nameof(StateColorType), typeof(StateColorType), typeof(EntityRow), StateColorType.Default);

    public StateColorType StateColorType
    {
        get => (StateColorType)GetValue(StateColorTypeProperty);
        set => SetValue(StateColorTypeProperty, value);
    }

    private void UpdateStateColorType()
    {
        try
        {
            var isActive = IsActive || (State?.ToLower() == "on" || State?.ToLower() == "open");
            
            StateColorType colorType = isActive ? StateColorType.Success :
                (State?.ToLower() == "unavailable" ? StateColorType.Error : StateColorType.Default);
            
            StateColorType = colorType;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EntityRow.UpdateStateColorType failed: {ex}");
        }
    }
}