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
                position: 'fixed', left: '0', top: '5%', height: '90%', width: '30%', minWidth: '200px',
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
                if (window.HassJsBridge && window.HassJsBridge.OpenVideoPlayer) {{
                    window.HassJsBridge.OpenVideoPlayer(videoUrl);
                }} else {{
                    console.error('HassJsBridge.OpenVideoPlayer not found.');
                }}
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

        public static void VideoSeek(HassWebView webView, int sencond)
        {
            webView.EvaluateJavaScriptAsync($@"(function() {{
                    function findFirstVideo(doc) {{
                        let videos = Array.from(doc.getElementsByTagName('video'))
                            .filter(v => v.src && v.src.trim() !== '');
                        if (videos.length > 0) return videos[0];

                        let iframes = doc.getElementsByTagName('iframe');
                        for (let i = 0; i < iframes.length; i++) {{
                            try {{
                                let idoc = iframes[i].contentDocument || iframes[i].contentWindow.document;
                                if (idoc) {{
                                    let v = findFirstVideo(idoc);
                                    if (v) return v;
                                }}
                            }} catch (e) {{

                            }}
                        }}
                        return null;
                    }}
                    var video = findFirstVideo(document);
                    if (video) video.currentTime += {sencond};
                }})()");
        }

        public static async Task TogglePlayPause(HassWebView webView)
        {
#if ANDROID
                if(webView.Handler.PlatformView is Com.Tencent.Smtt.Sdk.WebView wv)
                {
                    if (wv.WebChromeClient is Platforms.Android.WebChromeClientHandler wc)
                    {
                        var view = wc._customView;
                        var x = (int)webView.Width / 2;
                                                var y = (int)webView.Height / 2;
                        long downTime = Android.OS.SystemClock.UptimeMillis();
                        long eventTime = downTime + 50;

                        // 按下事件
                        var downEvent = Android.Views.MotionEvent.Obtain(downTime, eventTime, Android.Views.MotionEventActions.Down, x, y, 0);
                        // 抬起事件
                        var upEvent = Android.Views.MotionEvent.Obtain(downTime, eventTime + 50, Android.Views.MotionEventActions.Up, x, y, 0);

                        // 派发事件到 view
                        view.DispatchTouchEvent(downEvent);
                        view.DispatchTouchEvent(upEvent);
                    }
                }
#endif
        }
    }
}
