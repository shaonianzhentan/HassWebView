using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Views;
using System.Diagnostics;
using System.Web;

namespace HassWebView.Demo
{
    public class EchoData
    {
        public string Message { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Button_Clicked(null, null);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var deviceId = "test";
            var pushUrl = "http://localhost:8123/api/haapp";

            Shell.Current.GoToAsync($"/{nameof(HassPage)}?deviceId={Uri.EscapeDataString(deviceId)}&pushUrl={Uri.EscapeDataString(pushUrl)}");
        }
    }
}
