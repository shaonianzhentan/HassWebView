using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace HassWebView.Core.Services
{
    public class CursorControl
    {
        readonly View _cursor;        // 光标
        readonly AbsoluteLayout _root; // 父布局
        readonly HassWebView _wv;      // WebView
        public double Step { get; set; }

        public CursorControl(View cursor, AbsoluteLayout root, HassWebView wv, double step = 20)
        {
            _cursor = cursor;
            _root = root;
            _wv = wv;
            Step = step;
        }

        // 获取光标当前位置（相对于 AbsoluteLayout）
        public double X
        {
            get => AbsoluteLayout.GetLayoutBounds(_cursor).X;
            set
            {
                var rect = AbsoluteLayout.GetLayoutBounds(_cursor);
                AbsoluteLayout.SetLayoutBounds(_cursor,
                    new Rect(value, rect.Y, rect.Width, rect.Height));
            }
        }

        public double Y
        {
            get => AbsoluteLayout.GetLayoutBounds(_cursor).Y;
            set
            {
                var rect = AbsoluteLayout.GetLayoutBounds(_cursor);
                AbsoluteLayout.SetLayoutBounds(_cursor,
                    new Rect(rect.X, value, rect.Width, rect.Height));
            }
        }

        // ↑ ↓ ← →
        public void MoveUp() => MoveBy(0, -Step);
        public void MoveDown() => MoveBy(0, Step);
        public void MoveLeft() => MoveBy(-Step, 0);
        public void MoveRight() => MoveBy(Step, 0);

        public void MoveUpBy(int y=60){
            MoveUp();
            if (Y < _cursor.Height)
            {
                _wv.EvaluateJavaScriptAsync($"window.scrollBy(0, -{y})");
            }
        }
        public void MoveDownBy(int y = 60)
        {
            MoveDown();
            if (Y > _root.Height - (_cursor.Height + 5))
            {
                _wv.EvaluateJavaScriptAsync($"window.scrollBy(0, {y})");
            }
        }
        public void MoveLeftBy(int x = 60){
            MoveLeft();
            if (X < _cursor.Width)
            {
                _wv.EvaluateJavaScriptAsync($"window.scrollBy(-{x}, 0)");
            }
        }
        public void MoveRightBy(int x = 60)
        {
            MoveRight();
            if (X > _root.Width - (_cursor.Width + 5))
            {
                _wv.EvaluateJavaScriptAsync($"window.scrollBy({x}, 0)");
            }
        }

        public void MoveBy(double dx, double dy)
        {
            if (_root.Width <= 0 || _root.Height <= 0)
                return;

            double newX = X + dx;
            double newY = Y + dy;

            // Clamp，保证光标在父布局内
            newX = Math.Clamp(newX, 0, _root.Width - _cursor.Width);
            newY = Math.Clamp(newY, 0, _root.Height - _cursor.Height);

            X = newX;
            Y = newY;
        }

        public void Click()
        {
            _wv.SimulateTouch((int)X, (int)Y);
        }

        public async Task DoubleClick(int delay=100)
        {
            Click();
            await Task.Delay(delay); // Wait 100ms between clicks
            Click();
        }

        // Page scroll up
        public void SlideUp(double factor = 0.4, int duration = 300)
        {
            var x = (int)X;
            var y = (int)Y;
            var distance = (int)(_wv.Height * factor);
            _wv.SimulateTouchSlide(x, y, x, y + distance, duration);
        }

        // Page scroll down
        public void SlideDown(double factor = 0.4, int duration = 300)
        {
            var x = (int)X;
            var y = (int)Y;
            var distance = (int)(_wv.Height * factor);
            _wv.SimulateTouchSlide(x, y, x, y - distance, duration);
        }

        // Page scroll left
        public void SlideLeft(double factor = 0.4, int duration = 300)
        {
            var x = (int)X;
            var y = (int)Y;
            var distance = (int)(_wv.Width * factor);
            _wv.SimulateTouchSlide(x, y, x + distance, y, duration);
        }

        // Page scroll right
        public void SlideRight(double factor = 0.4, int duration = 300)
        {
            var x = (int)X;
            var y = (int)Y;
            var distance = (int)(_wv.Width * factor);
            _wv.SimulateTouchSlide(x, y, x - distance, y, duration);
        }


        public void VideoSeek(int sencond)
        {
            _wv.EvaluateJavaScriptAsync($@"(function() {{
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

        public async Task VideoPlayPause()
        {
#if ANDROID
                if(_wv.Handler.PlatformView is Com.Tencent.Smtt.Sdk.WebView wv)
                {
                    if (wv.WebChromeClient is Platforms.Android.WebChromeClientHandler wc)
                    {
                        var view = wc._customView;
                        var x = (int)_wv.Width / 2;
                                                var y = (int)_wv.Height / 2;
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
