using Microsoft.Maui.Controls;

namespace Maui.HassWebView.Core
{
    public class CursorControl
    {
        readonly View _cursor;        // 光标
        readonly AbsoluteLayout _root; // 父布局
        public double Step { get; set; }

        public CursorControl(View cursor, AbsoluteLayout root, double step = 20)
        {
            _cursor = cursor;
            _root = root;
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
    }
}
