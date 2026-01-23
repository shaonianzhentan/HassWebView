namespace HassWebView.Demo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MediaPage), typeof(MediaPage));
            Routing.RegisterRoute(nameof(WebPage), typeof(WebPage));
        }
    }
}
