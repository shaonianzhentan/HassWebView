namespace HassWebView.DemoComponent.Views;

using HassWebView.Component.Services;

public partial class AboutPage : ContentPage
{
    private readonly AppInfoService _appInfoService;

    public AboutPage()
    {
        InitializeComponent();
        _appInfoService = AppInfoService.Instance;
        BindingContext = _appInfoService;
        InitializeData();
    }

    private void InitializeData()
    {
        AppNameLabel.Text = _appInfoService.AppName;
        CompletedGauge.Value = _appInfoService.CompletedComponents;
        CompletedGauge.Maximum = _appInfoService.TotalComponents;
        ProgressGauge.Value = _appInfoService.TotalComponents - _appInfoService.CompletedComponents;
        ProgressGauge.Maximum = _appInfoService.TotalComponents;
    }

    private async void OnGitHubClicked(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(new Uri(_appInfoService.GitHubUrl));
        }
        catch (Exception)
        {
            await DisplayAlert("提示", "无法打开链接", "确定");
        }
    }
}