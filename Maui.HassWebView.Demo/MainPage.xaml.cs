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
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("MainPage Disappearing: Unsubscribing from KeyService events.");
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            wv.Navigating -= Wv_Navigating;
            wv.Navigated -= Wv_Navigated;
        }

        // Parameter type updated to RemoteKeyEventArgs
        private void HandleKeyEvent(string eventType, RemoteKeyEventArgs e)
        {
            if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
            {
                e.Handled = false;
                return;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Debug.WriteLine($"--- {eventType}: {e.KeyName} ---");

                switch (e.KeyName)
                {
                    // OK / Enter Key
                    case "Enter":
                    case "DpadCenter":
                        if (wv.IsVideoFullscreen)
                        {
                            cursorControl.VideoPlayPause();
                        }
                        else
                        {
                            if (eventType == "DoubleClick")
                                await cursorControl.DoubleClick();
                            else // SingleClick
                                cursorControl.Click();
                        }
                        break;

                    // Back / Escape Key
                    case "Escape":
                    case "Back":
                        if (eventType == "SingleClick")
                        {
                            if (wv.IsVideoFullscreen)
                                wv.ExitFullscreen();
                            else if (wv.CanGoBack)
                                wv.GoBack();
                            else
                                e.Handled = false; // Let system handle it
                        }
                        break;
                    
                    // Directional Keys (Up, Down, Left, Right)
                    case "Up":
                    case "DpadUp":
                        if (eventType == "SingleClick") cursorControl.MoveUp();
                        break;

                    case "Down":
                    case "DpadDown":
                        if (eventType == "SingleClick") cursorControl.MoveDown();
                        break;
                        
                    case "Left":
                    case "DpadLeft":
                         if (eventType == "SingleClick")
                         {
                            if(wv.IsVideoFullscreen)
                                cursorControl.VideoSeek(-5);
                            else
                                cursorControl.MoveLeft();
                         }
                        break;

                    case "Right":
                    case "DpadRight":
                        if (eventType == "SingleClick")
                        {
                            if(wv.IsVideoFullscreen)
                                cursorControl.VideoSeek(5);
                            else
                                cursorControl.MoveRight();
                        }
                        break;

                    // Unhandled Keys
                    default:
                        if (eventType == "SingleClick")
                        {
                            e.Handled = false; 
                            Debug.WriteLine($"Unhandled {eventType} for {e.KeyName}. Passing to system.");
                        }
                        break;
                }
            });
        }

        // Parameter type updated to RemoteKeyEventArgs
        private void OnSingleClick(RemoteKeyEventArgs e)
        {
            HandleKeyEvent("SingleClick", e);
        }

        // Parameter type updated to RemoteKeyEventArgs
        private void OnDoubleClick(RemoteKeyEventArgs e)
        {
            HandleKeyEvent("DoubleClick", e);
        }

        // Parameter type updated to RemoteKeyEventArgs
        private void OnLongClick(RemoteKeyEventArgs e)
        {
            if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
            {
                e.Handled = false;
                return;
            }

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
