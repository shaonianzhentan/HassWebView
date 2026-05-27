namespace HassWebView.Component;

using HassWebView.Component.Models;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// 初始化主题为系统默认
		ThemeManager.SetTheme(ThemeManager.CurrentTheme);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}