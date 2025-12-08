using HassWebView.Core;
using System.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Behaviors;
using HassWebView.Core.Bridges;

namespace HassWebView.Demo
{
    public class EchoData
    {
        public string Message { get; set; }
    }

    public partial class MainPage : ContentPage, IKeyHandler
    {
        private readonly HttpServer _httpServer;
        private CursorControl _cursorControl;

        public MainPage()
        {
            InitializeComponent();

            // Register the cross-platform JavaScript bridge
            wv.JsBridges.Add("externalApp", new ExternalApp((type, msg) =>
            {
                switch (type)
                {
                    case "getExternalAuth":
                        // 处理获取授权逻辑
                        break;
                    case "revokeExternalAuth":
                        // 处理撤销授权逻辑
                        break;
                }
            }));

            // Add the behavior programmatically
            this.Behaviors.Add(new RemoteControlBehavior());

            // Your original event subscriptions
            wv.Navigating += Wv_Navigating;
            wv.Navigated += Wv_Navigated;
            wv.ResourceLoading += Wv_ResourceLoading;
            Loaded += MainPage_Loaded;
            _cursorControl = new CursorControl(cursor, root, wv);

            _httpServer = new HttpServer(8080);
        }

        // Your original WebView and Page lifecycle methods
        private void Wv_ResourceLoading(object? sender, ResourceLoadingEventArgs e)
        {
            Debug.WriteLine($"ResourceLoading: {e.Url}");
            var urlString = e.Url.ToString();
            // 如果当前是视频页面，则阻止拦截
            if (wv.Source is UrlWebViewSource source && source.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                (source.Url.Contains(".mp4", StringComparison.OrdinalIgnoreCase) || 
                 source.Url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) || 
                 urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
            {
                Debug.WriteLine($"Video resource detected: {urlString}. Adding to video panel.");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await VideoService.AddVideo(wv, urlString);
                });
            }
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
            _httpServer.Get("/", async (HttpServer.Request req, HttpServer.Response res) =>
            {
                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("index.html");
                    using var reader = new StreamReader(stream);
                    var htmlContent = await reader.ReadToEndAsync();
                    await res.Html(htmlContent);
                }
                catch (Exception ex)
                {
                    await res.Text($"Could not load page. Error: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
                }
            });

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

            MainThread.BeginInvokeOnMainThread(() => {
                wv.Source = $"http://{HttpServer.GetLocalIPv4Address()}:8080/";
                Debug.WriteLine(wv.Source);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _httpServer.Stop();
        }

        // --- IKeyHandler Implementation ---

        public bool OnKeyDown(KeyService sender, RemoteKeyEventArgs args)
        {
            if (args.KeyName == "VolumeUp" || args.KeyName == "VolumeDown")
            {
                return false; // Let the system handle volume keys
            }
            return true; // We will handle all other keys
        }

        public void OnKeyUp(KeyService sender, RemoteKeyEventArgs args)
        {
            Debug.WriteLine($"--- OnKeyUp: {args.KeyName} ---");
            sender.StopRepeatingAction();
        }

        public void OnSingleClick(KeyService sender, RemoteKeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        if (wv.IsVideoFullscreen)
                           await VideoService.TogglePlayPause(wv);
                        else
                            _cursorControl.Click();
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
                        _cursorControl.MoveUpBy();
                        break;

                    case "Down":
                    case "DpadDown":
                        _cursorControl.MoveDownBy();
                        break;

                    case "Left":
                    case "DpadLeft":
                        if (wv.IsVideoFullscreen)
                            VideoService.VideoSeek(wv,-5);
                        else
                            _cursorControl.MoveLeftBy();
                        break;

                    case "Right":
                    case "DpadRight":
                        if (wv.IsVideoFullscreen)
                            VideoService.VideoSeek(wv,5);
                        else
                            _cursorControl.MoveRightBy();
                        break;
                    case "A":
                        wv.UserAgent = "Mozilla/5.0 (Linux; Android 14; Pixel 8 Build/UQ1A.240105.004; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/120.0.6099.210 Mobile Safari/537.36";
                        wv.Source = "https://www.baidu.com";
                        break;
                    case "B":
                        wv.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                        wv.Source = "https://www.baidu.com";
                        break;
                    case "M":
                        await VideoService.ToggleVideoPanel(wv);
                        break;
                }
            });
        }

        public void OnDoubleClick(KeyService sender, RemoteKeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        await _cursorControl.DoubleClick();
                        break;

                    case "Up":
                    case "DpadUp":
                        _cursorControl.SlideUp();
                        break;

                    case "Down":
                    case "DpadDown":
                        _cursorControl.SlideDown();
                        break;

                    case "Left":
                    case "DpadLeft":
                        _cursorControl.SlideLeft();
                        break;

                    case "Right":
                    case "DpadRight":
                        _cursorControl.SlideRight();
                        break;
                }
            });
        }

        public void OnLongClick(KeyService sender, RemoteKeyEventArgs e)
        {
            Debug.WriteLine($"--- OnLongClick: {e.KeyName} ---");
            int repeatInterval = 100;

            switch (e.KeyName)
            {
                case "Up":
                case "DpadUp":
                    sender.StartRepeatingAction(() => _cursorControl.MoveUpBy(), repeatInterval);
                    break;
                case "Down":
                case "DpadDown":
                    sender.StartRepeatingAction(() => _cursorControl.MoveDownBy(), repeatInterval);
                    break;
                case "Left":
                case "DpadLeft":
                    if (wv.IsVideoFullscreen)
                        VideoService.VideoSeek(wv,-15);
                    else
                        sender.StartRepeatingAction(() => _cursorControl.MoveLeftBy(), repeatInterval);
                    break;
                case "Right":
                case "DpadRight":
                    if (wv.IsVideoFullscreen)
                        VideoService.VideoSeek(wv,15);
                    else
                        sender.StartRepeatingAction(() => _cursorControl.MoveRightBy(), repeatInterval);
                    break;
            }
        }
    }
}
