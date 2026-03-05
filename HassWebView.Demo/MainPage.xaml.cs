using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Views;
using System.Diagnostics;
using System.Web;

namespace HassWebView.Demo
{

    public partial class MainPage : ContentPage
    {
        public MainPage(HttpServer httpServer)
        {
            InitializeComponent();

            Shell.Current.GoToAsync($"/{nameof(HassPage)}?pushUrl={Uri.EscapeDataString(httpServer.BaseUrl)}");
        }

    }
}
