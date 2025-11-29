using Maui.HassWebView.Core;
using System.Diagnostics;

namespace Maui.HassWebView.Demo
{
    public partial class MainPage : ContentPage
    {
        private readonly KeyService _keyService;
        CursorControl cursorControl;

        public MainPage(KeyService keyService)
        {
            InitializeComponent();
            _keyService = keyService; // Store the injected service

            // Subscribe to WebView navigation events
            wv.Navigating += Wv_Navigating;
            wv.Navigated += Wv_Navigated;

            Loaded += MainPage_Loaded;
            cursorControl = new CursorControl(cursor, root, wv);
        }

        private void Wv_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigating: {e.Url}");
        }

        private void Wv_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigated to: {e.Url}");
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            wv.Source = "http://debugx5.qq.com/";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("MainPage Appearing: Subscribing to KeyService events.");
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
            _keyService.Down += OnDown;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("MainPage Disappearing: Unsubscribing from KeyService events.");
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            _keyService.Down -= OnDown;
            wv.Navigating -= Wv_Navigating;
            wv.Navigated -= wv_Navigated;
        }

        private void HandleKeyEvent(string eventType, KeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Debug.WriteLine($"--- {eventType}: {e.KeyName} ---");

                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        if (wv.IsVideoFullscreen)
                        {
                            cursorControl.VideoPlayPause();
                        }
                        else
                        {
                            if (eventType == "DoubleClick")
                            {
                                await cursorControl.DoubleClick();
                            }
                            else // SingleClick
                            {
                                cursorControl.Click();
                            }
                        }
                        break;

                    case "Escape":
                    case "Back":
                        if (eventType == "SingleClick")
                        {
                            if (wv.IsVideoFullscreen)
                            {
                                wv.ExitFullscreen();
                            }
                            else if (wv.CanGoBack)
                            {
                                wv.GoBack();
                            }
                            else
                            {
                                e.Handled = false; // Let the system handle it (e.g., exit app)
                            }
                        }
                        break;

                    case "Left":
                    case "DpadLeft":
                         if (eventType == "SingleClick" && wv.IsVideoFullscreen)
                         {
                            cursorControl.VideoSeek(-5);
                         }
                        break;

                    case "Right":
                    case "DpadRight":
                        if (eventType == "SingleClick" && wv.IsVideoFullscreen)
                        {
                            cursorControl.VideoSeek(5);
                        }
                        break;

                    case "Up":
                    case "DpadUp":
                    case "Down":
                    case "DpadDown":
                        // Intercept clicks to prevent system sounds, but do nothing.
                        // The main logic is in OnDown/OnLongClick.
                        break;

                    default:
                        // For any other unhandled key, pass it to the system.
                        e.Handled = false; 
                        Debug.WriteLine($"Unhandled {eventType} for {e.KeyName}. Passing to system.");
                        break;
                }
            });
        }

        private void OnSingleClick(KeyEventArgs e)
        {
            HandleKeyEvent("SingleClick", e);
        }

        private void OnDoubleClick(KeyEventArgs e)
        {
            HandleKeyEvent("DoubleClick", e);
        }

        private void OnDown(KeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (e.KeyName)
                {
                    case "Up":
                    case "DpadUp":
                        cursorControl.MoveUp();
                        break;
                    case "Down":
                    case "DpadDown":
                        cursorControl.MoveDown();
                        break;
                    case "Left":
                    case "DpadLeft":
                        if (!wv.IsVideoFullscreen)
                            cursorControl.MoveLeft();
                        break;
                    case "Right":
                    case "DpadRight":
                        if (!wv.IsVideoFullscreen)
                            cursorControl.MoveRight();
                        break;
                }
            });
        }

        private void OnLongClick(KeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine($"--- OnLongClick: {e.KeyName} ---");
                switch (e.KeyName)
                {
                    case "Up":
                    case "DpadUp":
                        cursorControl.SlideUp();
                        break;
                    case "Down":
                    case "DpadDown":
                        cursorControl.SlideDown();
                        break;
                    case "Left":
                    case "DpadLeft":
                        cursorControl.SlideLeft();
                        break;
                    case "Right":
                    case "DpadRight":
                        cursorControl.SlideRight();
                        break;
                }
            });
        }
    }
}
