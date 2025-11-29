using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Maui.HassWebView.Core
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
                    var video = document.querySelector('video');
                    if (video) video.currentTime += {sencond};
                }})()");
        }

        public void VideoPlayPause()
        {
            _wv.EvaluateJavaScriptAsync($@"(function() {{
                    const video = document.fullscreenElement?.querySelector('video') || 
                                    (document.fullscreenElement?.tagName === 'VIDEO' ? document.fullscreenElement : null) || 
                                    document.querySelector('video');
                    if (video) video.paused ? video.play() : video.pause();
                }})()");
        }
    }
}
