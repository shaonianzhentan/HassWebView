namespace HassWebView.DemoAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnViewAllTapped(object? sender, EventArgs e)
        {
            // TODO: navigate to full device list
        }

        private void OnBrightnessChanged(object? sender, ValueChangedEventArgs e)
        {
            // e.NewValue contains the snapped brightness value
        }
    }
}
