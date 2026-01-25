
namespace HassWebView.Core.Views;

[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(Mode), "mode")]
public partial class HassAuthPage : ContentPage
{
    public string Url { get; set; }
    public string Mode { get; set; }

    public HassAuthPage()
	{
		InitializeComponent();
	}
}