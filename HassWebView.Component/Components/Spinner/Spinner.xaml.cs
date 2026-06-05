namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class Spinner : AdaptiveComponent
{
    public static readonly BindableProperty IsRunningProperty =
        BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(Spinner), true);

    public Spinner()
    {
        InitializeComponent();
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
}
