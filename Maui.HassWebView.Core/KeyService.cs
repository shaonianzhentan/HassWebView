
using System;
using System.Timers;

namespace Maui.HassWebView.Core
{
    /// <summary>
    /// 按键服务，用于区分单击、双击和长按操作。
    /// </summary>
    public class KeyService : IDisposable
    {
        /// <summary>
        /// 单击事件, 参数为键名字符串
        /// </summary>
        public event Action<string> SingleClick;

        /// <summary>
        /// 双击事件, 参数为键名字符串
        /// </summary>
        public event Action<string> DoubleClick;

        /// <summary>
        /// 长按事件, 参数为键名字符串
        /// </summary>
        public event Action<string> LongClick;

        private readonly Timer _longPressTimer;
        private readonly Timer _doubleClickTimer;
        private bool _isLongPress = false;
        private bool _isPressed = false; // 跟踪按键是否已被按下
        private string _currentKeyName; // 存储当前按下的键名字符串

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="longPressTimeout">长按超时时间（毫秒）</param>
        /// <param name="doubleClickTimeout">双击超时时间（毫秒）</param>
        public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300)
        {
            _longPressTimer = new Timer(longPressTimeout);
            _longPressTimer.Elapsed += OnLongPressTimerElapsed;
            _longPressTimer.AutoReset = false;

            _doubleClickTimer = new Timer(doubleClickTimeout);
            _doubleClickTimer.Elapsed += OnDoubleClickTimerElapsed;
            _doubleClickTimer.AutoReset = false;
        }

        /// <summary>
        /// 按键按下时由平台代码调用
        /// </summary>
        internal void OnPressed(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return;

            if (_isPressed) return; // 如果已按下，则忽略后续的按下事件
            _isPressed = true;
            _currentKeyName = keyName; // 存储键名

            _isLongPress = false;
            _longPressTimer.Start();

            if (_doubleClickTimer.Enabled)
            {
                // 检测到双击
                _doubleClickTimer.Stop();
                DoubleClick?.Invoke(_currentKeyName);
            }
            else
            {
                // 可能是单击的开始
                _doubleClickTimer.Start();
            }
        }

        /// <summary>
        /// 按键释放时由平台代码调用
        /// </summary>
        internal void OnReleased()
        {
            if (!_isPressed) return; // 只有在按下的状态才处理释放事件
            
            _longPressTimer.Stop();
            _isPressed = false;
        }

        private void OnLongPressTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _isLongPress = true;
            LongClick?.Invoke(_currentKeyName);
            // 长按发生，取消单击检测
            _doubleClickTimer.Stop();
        }

        private void OnDoubleClickTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isLongPress)
            {
                SingleClick?.Invoke(_currentKeyName);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _longPressTimer.Dispose();
            _doubleClickTimer.Dispose();
        }
    }
}
