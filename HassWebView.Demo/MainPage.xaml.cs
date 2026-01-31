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
        private readonly HttpServer _httpServer;

        public MainPage(HttpServer httpServer)
        {
            InitializeComponent();
            _httpServer = httpServer;
            Button_Clicked(null, null);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync($"/{nameof(HassPage)}?pushUrl={Uri.EscapeDataString(_httpServer.BaseUrl)}");
        }
    }
}
