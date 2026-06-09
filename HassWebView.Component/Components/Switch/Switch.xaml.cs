namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class Switch : AdaptiveComponent
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(
            nameof(IsToggled), 
            typeof(bool), 
            typeof(Switch), 
            false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnIsToggledPropertyChanged);

    private static void OnIsToggledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Switch customSwitch && newValue is bool val)
        {
            if (customSwitch.NativeSwitch.IsToggled != val)
            {
                customSwitch.NativeSwitch.IsToggled = val;
            }
        }
    }

    public Switch()
    {
        InitializeComponent();
        NativeSwitch.Toggled += NativeSwitch_Toggled;
    }

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public event EventHandler<ToggledEventArgs>? Toggled;

    private void NativeSwitch_Toggled(object? sender, ToggledEventArgs e)
    {
        IsToggled = e.Value;
        Toggled?.Invoke(this, e);
    }
}