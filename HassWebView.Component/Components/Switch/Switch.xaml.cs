namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class Switch : AdaptiveComponent
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), false);

    public Switch()
    {
        InitializeComponent();
        NativeSwitch.Toggled += NativeSwitch_Toggled;
    }

    public bool IsToggled
    {
        get => NativeSwitch.IsToggled;
        set => NativeSwitch.IsToggled = value;
    }

    public event EventHandler<ToggledEventArgs>? Toggled;

    private void NativeSwitch_Toggled(object? sender, ToggledEventArgs e)
    {
        Toggled?.Invoke(this, e);
    }
}