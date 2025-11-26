namespace Maui.HassWebView.Demo
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            wv.Source= new UrlWebViewSource
            {
                Url = "https://github.com/shaonianzhentan"
            };
        }
    }
}
