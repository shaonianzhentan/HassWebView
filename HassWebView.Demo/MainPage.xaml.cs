using System.Diagnostics;
using HassWebView.Core.Events;
using HassWebView.Core.Services;
using HassWebView.Core.Bridges;

namespace HassWebView.Demo
{
    public class EchoData
    {
        public string Message { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly HttpServer _httpServer;
        private readonly KeyService _keyService;
        private CursorControl _cursorControl;

        public MainPage(KeyService keyService)
        {
            InitializeComponent();
            _keyService = keyService;

            // Register the cross-platform JavaScript bridge
            wv.JsBridges.Add("externalApp", new ExternalApp((type, msg) =>
            {
                switch (type)
                {
                    case "getExternalAuth":
                        // Handle authorization logic
                        break;
                    case "revokeExternalAuth":
                        // Handle revocation logic
                        break;
                }
            }));
            wv.JsBridges.Add("HassJsBridge", new HassJsBridge(async (type, msg) =>
            {
                switch (type)
                {
                    case "OpenVideoPlayer":
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}/{nameof(MediaPage)}?Url={Uri.EscapeDataString(msg)}");
                        });
                        break;
                }
            }));

            // Subscribe to WebView and Page lifecycle events
            wv.Navigating += Wv_Navigating;
            wv.Navigated += Wv_Navigated;
            wv.ResourceLoading += Wv_ResourceLoading;
            Loaded += MainPage_Loaded;

            _cursorControl = new CursorControl(cursor, root, wv);
            _httpServer = new HttpServer(8080);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Subscribe to KeyService events
            _keyService.KeyDown += OnKeyDown;
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
        }

        private void Wv_ResourceLoading(object? sender, ResourceLoadingEventArgs e)
        {
            Debug.WriteLine($"ResourceLoading: {e.Url}");
            var urlString = e.Url.ToString();
            if (wv.Source is HtmlWebViewSource) return;

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
            catch (Exception) { /* Invalid URL */ }
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
            _httpServer.Get("/", async (req, res) =>
            {
                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("index.html");
                    using var reader = new StreamReader(stream);
                    await res.Html(await reader.ReadToEndAsync());
                }
                catch (Exception ex)
                {
                    await res.Text($"Could not load page. Error: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
                }
            });

            _httpServer.Get("/api/info", async (req, res) =>
            {
                var name = req.Query["name"] ?? "World";
                var data = new { Message = $"Hello, {name}!", LocalIP = HttpServer.GetLocalIPv4Address(), CurrentTime = DateTime.Now };
                await res.Json(data);
            });

            _httpServer.Post("/api/echo", async (req, res) =>
            {
                var receivedData = await req.JsonAsync<EchoData>();
                var responseData = new { ReceivedMessage = receivedData.Message, Timestamp = DateTime.Now };
                await res.Json(responseData);
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
            // Unsubscribe from KeyService events to prevent memory leaks
            _keyService.KeyDown -= OnKeyDown;
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
        }

        // --- Key Event Handlers ---

        private bool OnKeyDown(object sender, RemoteKeyEventArgs args)
        {
            if (args.KeyName == "VolumeUp" || args.KeyName == "VolumeDown")
            {
                return false; // Let the system handle volume keys
            }
            return true; // We will handle all other keys
        }

        private void OnSingleClick(object sender, RemoteKeyEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                switch (e.KeyName)
                {
                    case "Enter":
                    case "DpadCenter":
                        _cursorControl.Click();
                        break;
                    case "Escape":
                    case "Back":
                        if (wv.IsVideoFullscreen) wv.ExitFullscreen();
                        else if (wv.CanGoBack) wv.GoBack();
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
                        _cursorControl.MoveLeftBy();
                        break;
                    case "Right":
                    case "DpadRight":
                        _cursorControl.MoveRightBy();
                        break;
                    case "M":
                        await VideoService.ToggleVideoPanel(wv);
                        break;
                }
            });
        }

        private void OnDoubleClick(object sender, RemoteKeyEventArgs e)
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

        private void OnLongClick(object sender, RemoteKeyEventArgs e)
        {
            Debug.WriteLine($"--- OnLongClick: {e.KeyName} ---");
            int repeatInterval = 100;

            switch (e.KeyName)
            {
                case "Up":
                case "DpadUp":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveUpBy(), repeatInterval);
                    break;
                case "Down":
                case "DpadDown":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveDownBy(), repeatInterval);
                    break;
                case "Left":
                case "DpadLeft":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveLeftBy(), repeatInterval);
                    break;
                case "Right":
                case "DpadRight":
                    _keyService.StartRepeatingAction(() => _cursorControl.MoveRightBy(), repeatInterval);
                    break;
            }
        }
    }
}
