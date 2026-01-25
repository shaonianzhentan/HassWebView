using HassWebView.Core.Views;

namespace HassWebView.Demo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(HassMediaPage), typeof(HassMediaPage));
            Routing.RegisterRoute(nameof(HassAuthPage), typeof(HassAuthPage));
        }
    }
}
