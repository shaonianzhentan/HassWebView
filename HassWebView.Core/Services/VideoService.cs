using System.Threading.Tasks;

namespace HassWebView.Core.Services
{
    public static class VideoService
    {
        public static Task AddVideo(HassWebView webView, string url)
        {
            var script = @$"
(function() {{
    function addVideoToPanel(videoUrl) {{
        const safeId = 'video-panel-item-' + encodeURIComponent(videoUrl).replace(/[^a-zA-Z0-9_-]/g, '_');
        let container = document.getElementById('video-panel-container');

        if (!container) {{
            container = document.createElement('div');
            container.id = 'video-panel-container';
            Object.assign(container.style, {{
                position: 'fixed', left: 0, top: 0, height: '100%', width: '30%', minWidth: '200px',
                display: 'flex', flexDirection: 'column', gap: '8px', padding: '10px',
                backgroundColor: 'rgba(0,0,0,0.6)', borderRadius: '0 10px 10px 0',
                boxSizing: 'border-box', zIndex: '9999', overflowY: 'auto'
            }});
            document.body.appendChild(container);
        }}

        let item = document.getElementById(safeId);

        if (!item) {{
            item = document.createElement('div');
            item.id = safeId;
            item.textContent = videoUrl;
            Object.assign(item.style, {{
                padding: '8px', backgroundColor: 'rgba(255, 255, 255, 0.1)',
                color: '#ffffff', border: '1px solid #555', borderRadius: '5px',
                wordBreak: 'break-all', cursor: 'pointer'
            }});

            item.addEventListener('click', () => {{
                //window.location.href = videoUrl;
                window.HassJsBridge.OpenVideoPlayer(videoUrl)
            }});
        }}

        container.insertBefore(item, container.firstChild);

        while (container.children.length > 10) {{
            container.removeChild(container.lastChild);
        }}
    }}
    
    addVideoToPanel('{url}');
}})();";
            
            return webView.EvaluateJavaScriptAsync(script);
        }

        public static Task ToggleVideoPanel(HassWebView webView)
        {
            var script = "(function() { var div = document.getElementById('video-panel-container'); if (div) { div.style.display = div.style.display === 'none' ? 'flex' : 'none'; } })();";
            return webView.EvaluateJavaScriptAsync(script);
        }

        public static void HtmlWebView(HassWebView webView, string videoUrl){
            // 使用 CSS 实现视频全屏铺满
            string htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{ margin: 0; padding: 0; background-color: black; }}
                        video {{
                            width: 100vw;
                            height: 100vh;
                            object-fit: contain; /* 保持比例，覆盖整个视口，可以改为 'cover' 如果想裁剪 */
                        }}
                    </style>
                </head>
                <body>
                    <video controls autoplay src='{videoUrl}'></video>
                </body>
                </html>";

            // 2. 创建 HtmlWebViewSource
            var htmlSource = new HtmlWebViewSource
            {
                Html = htmlContent,
                
                // BaseUrl 可以是 null 或 videoUrl 的域名，
                // 如果视频链接是绝对路径，这个属性影响不大，但最好设置以满足同源策略。
                BaseUrl = videoUrl.Contains("http") ? new Uri(videoUrl).GetLeftPart(UriPartial.Authority) : null
            };

            // 3. 将 Source 赋值给你的 WebView 控件
            // 假设你的 WebView 控件名为 'videoWebView'
            MainThread.BeginInvokeOnMainThread(() =>
            {
                webView.Source = htmlSource;
            });
        }

        public static void VideoSeek(HassWebView webView, int sencond)
        {
#if ANDROID
            if (webView.Handler?.PlatformView is Com.Tencent.Smtt.Sdk.WebView wv &&
                wv.WebChromeClient is Platforms.Android.WebChromeClientHandler wc)
            {
                var view = wc._customView;
                if (view != null)
                {
                    var y = (int)(webView.Height / 2);
                    var startX = (int)(webView.Width / 2);
                    var swipeDistance = (int)(webView.Width / 4); // Swipe a quarter of the screen width
                    var endX = startX + (sencond > 0 ? swipeDistance : -swipeDistance);

                    long downTime = Android.OS.SystemClock.UptimeMillis();
                    
                    var downEvent = Android.Views.MotionEvent.Obtain(downTime, downTime, Android.Views.MotionEventActions.Down, startX, y, 0);
                    view.DispatchTouchEvent(downEvent);
                    
                    var moveEvent = Android.Views.MotionEvent.Obtain(downTime, downTime + 100, Android.Views.MotionEventActions.Move, endX, y, 0);
                    view.DispatchTouchEvent(moveEvent);

                    var upEvent = Android.Views.MotionEvent.Obtain(downTime, downTime + 150, Android.Views.MotionEventActions.Up, endX, y, 0);
                    view.DispatchTouchEvent(upEvent);

                    downEvent.Recycle();
                    moveEvent.Recycle();
                    upEvent.Recycle();
                    return; 
                }
            }
#endif
            // JS Fallback for embedded videos in a webpage
            webView.EvaluateJavaScriptAsync($@"(function() {{
                    var video = document.querySelector('video');
                    if (video) video.currentTime += {sencond};
                }})()");
        }

        public static async Task TogglePlayPause(HassWebView webView)
        {
#if ANDROID
            if (webView.Handler?.PlatformView is Com.Tencent.Smtt.Sdk.WebView wv &&
                wv.WebChromeClient is Platforms.Android.WebChromeClientHandler wc)
            {
                var view = wc._customView;
                if (view != null)
                {
                    var x = (int)(webView.Width / 2);
                    var y = (int)(webView.Height / 2);
                    long downTime = Android.OS.SystemClock.UptimeMillis();
                    
                    var downEvent = Android.Views.MotionEvent.Obtain(downTime, downTime + 50, Android.Views.MotionEventActions.Down, x, y, 0);
                    view.DispatchTouchEvent(downEvent);

                    var upEvent = Android.Views.MotionEvent.Obtain(downTime, downTime + 100, Android.Views.MotionEventActions.Up, x, y, 0);
                    view.DispatchTouchEvent(upEvent);

                    downEvent.Recycle();
                    upEvent.Recycle();
                    return;
                }
            }
#endif
            // JS Fallback for embedded videos in a webpage
            await webView.EvaluateJavaScriptAsync(@"(function() {
                    var video = document.querySelector('video');
                    if (video) {
                        if (video.paused) {
                            video.play();
                        } else {
                            video.pause();
                        }
                    }
                })()");
        }
    }
}
