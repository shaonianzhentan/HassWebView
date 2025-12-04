
using HassWebView.Core;
using System.Diagnostics;
using System;
using System.IO;
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
            wv.ResourceLoading += Wv_ResourceLoading;
            Loaded += MainPage_Loaded;
            cursorControl = new CursorControl(cursor, root, wv);

            _httpServer = new HttpServer(8080);
        }

        private void Wv_ResourceLoading(object? sender, ResourceLoadingEventArgs e)
        {
            Debug.WriteLine($"ResourceLoading: {e.Url}");
            var urlString = e.Url.ToString();

            // Corrected logic: check for http(s) and contains .mp4 or .m3u8
            if (urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                (urlString.Contains(".mp4", StringComparison.OrdinalIgnoreCase) || 
                 urlString.Contains(".m3u8", StringComparison.OrdinalIgnoreCase)))
            {
                Debug.WriteLine($"Video resource detected: {urlString}. Preparing to inject script and allowing request to continue.");

                // The request is NOT cancelled, as requested.

                // I am ready to add the JavaScript execution here. Please provide the script.
                // For example:
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var script = $"console.log('Video detected: {urlString}');";
                    wv.EvaluateJavaScriptAsync(@"(function(){
function createLeftFloatDivByUrl(url, displayText) {
    // 1. URL转义生成合法DOM ID（避免特殊字符报错）
    const safeId = 'float-' + encodeURIComponent(url).replace(/[^a-zA-Z0-9_-]/g, '_');
    
    // 2. 判重：已存在则直接返回
    if (document.getElementById(safeId)) {
        console.log(`URL【${url}】的悬浮项已存在，无需重复添加`);
        return;
    }

    // 3. 创建/获取左侧100%高度容器（单例），宽度设为30%
    let container = document.getElementById('left-float-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'left-float-container';
        Object.assign(container.style, {
            position: 'fixed',
            left: '0',
            top: '0',
            height: '100%',
            width: '30%', // 宽度调整为30%（适配不同屏幕）
            display: 'flex',
            flexDirection: 'column',
            gap: '8px',
            padding: '10px 0',
            boxSizing: 'border-box',
            zIndex: 9999
        });
        document.body.appendChild(container);
    }

    // 4. 创建单个悬浮项（黑色半透明背景 + 白色文字 + 5px内边距 + 强制换行）
    const floatItem = document.createElement('div');
    floatItem.id = safeId;
    floatItem.dataset.url = url; // 存储原始URL
    floatItem.textContent = displayText || url; // 优先显示自定义文本

    // 最终样式配置
    Object.assign(floatItem.style, {
        width: '100%', // 继承容器30%宽度，自身占满容器
        padding: '5px', // 上下左右均为5px
        backgroundColor: 'rgba(0, 0, 0, 0.7)', // 黑色半透明背景
        color: '#ffffff', // 白色字体
        border: '1px solid #ccc',
        boxSizing: 'border-box',
        // 强制换行核心样式
        wordBreak: 'break-all',
        wordWrap: 'break-word'
    });

    // 5. 点击事件：调用视频播放器桥接方法
    floatItem.addEventListener('click', () => {
        if (window.HassJsBridge && typeof window.HassJsBridge.OpenVideoPlayer === 'function') {
            window.HassJsBridge.OpenVideoPlayer(url);
            console.log(`调用视频播放器：${url}`);
        } else {
            console.error('HassJsBridge 未定义或 OpenVideoPlayer 方法不存在');
        }
    });

    // 6. 添加到容器
    container.appendChild(floatItem);
    console.log(`URL【${url}】的悬浮项已添加`);
    return floatItem;
}
                        
                        createLeftFloatDivByUrl('" + urlString + @"');
                        })();");
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

            wv.Source = $"http://{HttpServer.GetLocalIPv4Address()}:8080/";
            Debug.WriteLine(wv.Source);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("MainPage Appearing: Subscribing to KeyService events.");
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
            _keyService.KeyDown += _keyService_KeyDown;
            _keyService.KeyUp += OnKeyUp;
        }

        private void _keyService_KeyDown(RemoteKeyEventArgs e)
        {
            if (e.KeyName == "VolumeUp" || e.KeyName == "VolumeDown")
            {
                e.Handled = false;
                return;
            }
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
                    case "A":
                        wv.UserAgent = "Mozilla/5.0 (Linux; Android 14; Pixel 8 Build/UQ1A.240105.004; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/120.0.6099.210 Mobile Safari/537.36";
                        wv.Source = "https://www.baidu.com";
                        break;
                    case "B":
                        wv.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                        wv.Source = "https://www.baidu.com";
                        break;
                    case "M":
                        wv.EvaluateJavaScriptAsync(@"(()=>{
var div = document.getElementById('left-float-container')
if(div){
    div.style.display = div.style.display == 'block' ? 'none' : 'block'; 
}
})();");
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
