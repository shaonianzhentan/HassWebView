namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class ControlSwitch : SizeableComponent
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ControlSwitch), false,
            propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty DisabledProperty =
        BindableProperty.Create(nameof(Disabled), typeof(bool), typeof(ControlSwitch), false);

    public ControlSwitch()
    {
        InitializeComponent();
        UpdateThumbPosition(false);
    }

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public event EventHandler<ToggledEventArgs>? Toggled;

    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ControlSwitch switchControl)
        {
            switchControl.UpdateThumbPosition(true);
            switchControl.Toggled?.Invoke(switchControl, new ToggledEventArgs((bool)newValue));
        }
    }

    private void UpdateThumbPosition(bool animated = true)
    {
        try
        {
            double maxTranslation = SwitchGrid.WidthRequest - Thumb.WidthRequest - 4;
            double targetX = IsToggled ? maxTranslation : 0;

            if (animated)
            {
                _ = Thumb.TranslateToAsync(targetX, 0, 150, Easing.CubicInOut);
            }
            else
            {
                Thumb.TranslationX = targetX;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ControlSwitch.UpdateThumbPosition failed: {ex}");
        }
    }

    private void OnTapped(object sender, EventArgs e)
    {
        if (!Disabled)
        {
            IsToggled = !IsToggled;
        }
    }
}