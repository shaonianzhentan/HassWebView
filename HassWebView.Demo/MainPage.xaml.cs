
using HassWebView.Core;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using HassWebView.Core.Events;

namespace HassWebView.Demo
{
    public class EchoData
    {
        public string Message { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly KeyService _keyService;
        private readonly HttpServer _httpServer;
        private CursorControl cursorControl;

        public MainPage(KeyService keyService)
        {
            InitializeComponent();
            _keyService = keyService;

            wv.Navigating += Wv_Navigating;
            wv.Navigated += Wv_Navigated;
            Loaded += MainPage_Loaded;
            cursorControl = new CursorControl(cursor, root, wv);

            _httpServer = new HttpServer(8080);
        }

        private void Wv_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigating: {e.Url}");
            try
            {
                var host = new Uri(e.Url).Host;
                if (host.EndsWith("youtube.com"))
                {
                    wv.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                }
                else if (host.EndsWith("bilibili.com"))
                {
                    wv.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36";
                }
            }
            catch (Exception)
            {
                // Invalid URL
            }
        }

        private void Wv_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Debug.WriteLine($">>> WebView Navigated to: {e.Url}");
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            Task.Run(() => StartHttpServer());
        }

        private async Task StartHttpServer()
        {
            _httpServer.Get("/api/info", async (HttpServer.Request req, HttpServer.Response res) =>
            {
                var name = req.Query["name"] ?? "World";
                var data = new { Message = $"Hello, {name}!", LocalIP = HttpServer.GetLocalIPv4Address(), CurrentTime = DateTime.Now };
                await res.Json(data);
            });

            _httpServer.Post("/api/echo", async (HttpServer.Request req, HttpServer.Response res) =>
            {
                var receivedData = await req.JsonAsync<EchoData>();
                var responseData = new { ReceivedMessage = receivedData.Message, Timestamp = DateTime.Now };
                await res.Json(responseData);
            });

            _httpServer.Put("/api/data", async (HttpServer.Request req, HttpServer.Response res) =>
            {
                var rawBody = await req.BodyAsync();
                Debug.WriteLine($"Raw data received via PUT: {rawBody}");
                await res.Json(new { Status = "Success", Message = "Data received" });
            });

            await _httpServer.StartAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("MainPage Appearing: Subscribing to KeyService events.");
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
            _keyService.KeyUp += OnKeyUp;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("MainPage Disappearing: Unsubscribing from KeyService events.");
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
            _keyService.KeyUp -= OnKeyUp;
            wv.Navigating -= Wv_Navigating;
            wv.Navigated -= Wv_Navigated;
            _httpServer.Stop();
        }

        private void OnSingleClick(RemoteKeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        if (wv.IsVideoFullscreen)
                            cursorControl.VideoPlayPause();
                        else
                            cursorControl.Click();
                        break;

                    case "Escape":
                    case "Back":
                        if (wv.IsVideoFullscreen)
                            wv.ExitFullscreen();
                        else if (wv.CanGoBack)
                            wv.GoBack();
                        break;

                    case "Up":
                    case "DpadUp":
                        cursorControl.MoveUpBy();
                        break;

                    case "Down":
                    case "DpadDown":
                        cursorControl.MoveDownBy();
                        break;

                    case "Left":
                    case "DpadLeft":
                        if (wv.IsVideoFullscreen)
                            cursorControl.VideoSeek(-5);
                        else
                            cursorControl.MoveLeftBy();
                        break;

                    case "Right":
                    case "DpadRight":
                        if (wv.IsVideoFullscreen)
                            cursorControl.VideoSeek(5);
                        else
                            cursorControl.MoveRightBy();
                        break;
                }
            });
        }

        private void OnDoubleClick(RemoteKeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        await cursorControl.DoubleClick();
                        break;

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

        private void OnLongClick(RemoteKeyEventArgs e)
        {
            if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
            {
                e.Handled = false;
                return;
            }

            Debug.WriteLine($"--- OnLongClick: {e.KeyName} ---");
            int repeatInterval = 100;

            switch (e.KeyName)
            {
                case "Up":
                case "DpadUp":
                    _keyService.StartRepeatingAction(() => cursorControl.MoveUpBy(), repeatInterval);
                    break;
                case "Down":
                case "DpadDown":
                    _keyService.StartRepeatingAction(() => cursorControl.MoveDownBy(), repeatInterval);
                    break;
                case "Left":
                case "DpadLeft":
                    if (wv.IsVideoFullscreen)
                        cursorControl.VideoSeek(-15);
                    else
                        _keyService.StartRepeatingAction(() => cursorControl.MoveLeftBy(), repeatInterval);
                    break;
                case "Right":
                case "DpadRight":
                    if (wv.IsVideoFullscreen)
                        cursorControl.VideoSeek(15);
                    else
                        _keyService.StartRepeatingAction(() => cursorControl.MoveRightBy(), repeatInterval);
                    break;
            }
        }

        private void OnKeyUp(RemoteKeyEventArgs e)
        {
            Debug.WriteLine($"--- OnKeyUp: {e.KeyName} ---");
        }
    }
}
