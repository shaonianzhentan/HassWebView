using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Views;
using System.Diagnostics;
using System.Web;
using HassWebView.AndroidService.AdSkipping;

namespace HassWebView.Demo
{

    public partial class MainPage : ContentPage
    {
        private readonly IAdSkippingManager _adSkippingManager;

        public MainPage(HttpServer httpServer, IAdSkippingManager adSkippingManager)
        {
            InitializeComponent();
            _adSkippingManager = adSkippingManager;
        }

        private void OnAnalyticsToggled(object sender, ToggledEventArgs e)
        {
            bool isNowEnabled = e.Value;
            Debug.WriteLine($"Advanced Analytics is now: {(isNowEnabled ? "Enabled" : "Disabled")}");
        }

        private void AdSkippingClicked(object sender, ToggledEventArgs e)
        {
            if (_adSkippingManager.IsPermissionEnabled())
            {
                DisplayAlert("Success", "Permission is already enabled!", "OK");
            } 
            else
            {
                _adSkippingManager.RequestPermission();
            }
        }
    }
}
