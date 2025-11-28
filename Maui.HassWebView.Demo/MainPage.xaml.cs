
using Maui.HassWebView.Core;
using System.Diagnostics;

namespace Maui.HassWebView.Demo
{
    public partial class MainPage : ContentPage
    {
        private readonly KeyService _keyService;

        public MainPage(KeyService keyService)
        {
            InitializeComponent();
            _keyService = keyService; // Store the injected service

            // Subscribe to WebView navigation events
            wv.Navigating += Wv_Navigating;
            wv.Navigated += Wv_Navigated;

            Loaded += MainPage_Loaded;
        }

        private void Wv_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigating: {e.Url}");
            // Example of how to cancel navigation to a specific URL
            if (e.Url.Contains("microsoft.com"))
            {
                Debug.WriteLine(">>> Canceling navigation to Microsoft.com!");
                e.Cancel = true;
            }
        }

        private void Wv_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigated to: {e.Url}");
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            wv.Source = new UrlWebViewSource
            {
                Url = "https://github.com/shaonianzhentan"
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("MainPage Appearing: Subscribing to KeyService events.");
            // Subscribe to the events
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("MainPage Disappearing: Unsubscribing from KeyService events.");
            // Unsubscribe from the events to prevent memory leaks
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            
            // It's also good practice to unsubscribe from WebView events
            wv.Navigating -= Wv_Navigating;
            wv.Navigated -= Wv_Navigated;
        }

        private void HandleKeyEvent(string eventType, string keyName)
        {
            // IMPORTANT: Ensure UI updates are on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine($"--- MainPage: {eventType} Detected ---");
                Debug.WriteLine($"Key Name: {keyName}");

                // Handle the key name using a switch statement on the string
                switch (keyName)
                {
                    // You can handle multiple key names in one case for mapping
                    case "Enter":
                    case "DpadCenter":
                        Debug.WriteLine("Action: OK/Enter was pressed.");
                        // e.g., wv.EvaluateJavaScriptAsync("document.activeElement.click();");
                        break;

                    case "Escape":
                    case "Back":
                        Debug.WriteLine("Action: Back/Escape was pressed.");
                        if (wv.CanGoBack) { wv.GoBack(); }
                        break;

                    case "Up":
                    case "DpadUp":
                        Debug.WriteLine("Action: Up was pressed.");
                        break;

                    case "Down":
                    case "DpadDown":
                        Debug.WriteLine("Action: Down was pressed.");
                        break;

                    case "Left":
                    case "DpadLeft":
                        Debug.WriteLine("Action: Left was pressed.");
                        break;

                    case "Right":
                    case "DpadRight":
                        Debug.WriteLine("Action: Right was pressed.");
                        break;
                    
                    // Now you can handle ANY key without changing the Core library
                    case "Menu":
                        Debug.WriteLine("Action: Menu key was pressed.");
                        break;

                    case "ChannelUp":
                        Debug.WriteLine("Action: Channel Up key was pressed.");
                        break;

                    default:
                        Debug.WriteLine($"Action: An unhandled key '{keyName}' was pressed.");
                        break;
                }
            });
        }

        private void OnSingleClick(string keyName)
        {
            HandleKeyEvent("Single Click", keyName);
        }

        private void OnDoubleClick(string keyName)
        {
            HandleKeyEvent("Double Click", keyName);
        }

        private void OnLongClick(string keyName)
        {
            HandleKeyEvent("Long Click", keyName);
        }
    }
}
