namespace HassWebView.Component;

using HassWebView.Component.Models;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		UpdateHeaderSize();
		SizeManager.SizeChanged += (s, e) => UpdateHeaderSize();
	}

	private void UpdateHeaderSize()
	{
		switch (SizeManager.CurrentSize)
		{
			case ComponentSize.Phone:
				HeaderLabel.FontSize = 28;
				break;
			case ComponentSize.Tablet:
				HeaderLabel.FontSize = 42;
				break;
			case ComponentSize.TV:
				HeaderLabel.FontSize = 56;
				break;
		}
	}
}