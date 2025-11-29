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
            _keyService.KeyUp += OnKeyUp; // <-- SUBSCRIBED to KeyUp event
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("MainPage Disappearing: Unsubscribing from KeyService events.");
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            _keyService.KeyUp -= OnKeyUp; // <-- UNSUBSCRIBED from KeyUp event
            wv.Navigating -= Wv_Navigating;
            wv.Navigated -= Wv_Navigated;
        }

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

        private void OnSingleClick(RemoteKeyEventArgs e)
        {
            HandleKeyEvent("SingleClick", e);
        }

        private void OnDoubleClick(RemoteKeyEventArgs e)
        {
            HandleKeyEvent("DoubleClick", e);
        }

        // --- MODIFIED: OnLongClick now starts a repeating action ---
        private void OnLongClick(RemoteKeyEventArgs e)
        {
            if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
            {
                e.Handled = false;
                return;
            }

            Debug.WriteLine($"--- OnLongClick: {e.KeyName} ---");

            // The interval for repeating the action, in milliseconds.
            int repeatInterval = 100; 

            switch (e.KeyName)
            {
                case "Up":
                case "DpadUp":
                    _keyService.StartRepeatingAction(() => cursorControl.MoveUp(), repeatInterval);
                    break;
                case "Down":
                case "DpadDown":
                    _keyService.StartRepeatingAction(() => cursorControl.MoveDown(), repeatInterval);
                    break;
                case "Left":
                case "DpadLeft":
                    if (wv.IsVideoFullscreen) {
                        cursorControl.VideoSeek(-15);
                    } else {
                        _keyService.StartRepeatingAction(() => cursorControl.MoveLeft(), repeatInterval);
                    }
                    break;
                case "Right":
                case "DpadRight":
                     if (wv.IsVideoFullscreen) {
                        cursorControl.VideoSeek(15);
                    } else {
                        _keyService.StartRepeatingAction(() => cursorControl.MoveRight(), repeatInterval);
                    }
                    break;
            }
        }

        private void OnKeyUp(RemoteKeyEventArgs e)
        {
            Debug.WriteLine($"--- OnKeyUp: {e.KeyName} ---");
            // The KeyService automatically stops the repeating action on KeyUp.
        }
    }
}
