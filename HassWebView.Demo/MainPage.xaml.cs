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
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var url = "http://192.168.0.100:8123";
            Shell.Current.GoToAsync($"/{nameof(HassAuthPage)}?url={Uri.EscapeDataString(url)}");
        }
    }
}
