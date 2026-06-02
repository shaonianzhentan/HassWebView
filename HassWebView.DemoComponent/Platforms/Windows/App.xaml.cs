using Microsoft.Maui;
using Microsoft.UI.Xaml;

namespace HassWebView.DemoComponent.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => global::HassWebView.DemoComponent.MauiProgram.CreateMauiApp();
}