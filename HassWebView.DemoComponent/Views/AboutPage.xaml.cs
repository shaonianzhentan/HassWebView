namespace HassWebView.DemoComponent.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        InitializeData();
    }

    private void InitializeData()
    {
        AppNameLabel.Text = AppInfo.AppName;
        AppNameValue.Text = AppInfo.AppName;
        VersionValue.Text = AppInfo.Version;
        FrameworkValue.Text = AppInfo.Framework;
        DesignSystemValue.Text = AppInfo.DesignSystem;
    }
}