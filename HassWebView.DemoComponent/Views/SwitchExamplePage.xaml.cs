namespace HassWebView.DemoComponent.Views;

public partial class SwitchExamplePage : ContentPage
{
    public SwitchExamplePage()
    {
        InitializeComponent();
        BindableSwitch.Toggled += BindableSwitch_Toggled;
    }

    private void BindableSwitch_Toggled(object? sender, ToggledEventArgs e)
    {
        StatusLabel.Text = $"当前状态: {e.Value}";
    }
}