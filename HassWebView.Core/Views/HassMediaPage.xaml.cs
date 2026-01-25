namespace HassWebView.Core.Views;

[QueryProperty(nameof(BaseUrl), "BaseUrl")]
[QueryProperty(nameof(Url), "Url")]
public partial class HassMediaPage : ContentPage
{
    public string BaseUrl { get; set; }
    public string Url { get; set; }

    public HassMediaPage()
	{
		InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        wv.LoadUrl(Url, BaseUrl);
    }
}