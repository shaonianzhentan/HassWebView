using AndroidNative = Android;

namespace HassWebView.Core.Platforms.Android
{
    public class KeyHelper
    {
        public static string GetKeyName(AndroidNative.Views.Keycode keyCode)
        {
            return keyCode switch
            {
                AndroidNative.Views.Keycode.DpadUp => "DpadUp",
                AndroidNative.Views.Keycode.DpadDown => "DpadDown",
                AndroidNative.Views.Keycode.DpadLeft => "DpadLeft",
                AndroidNative.Views.Keycode.DpadRight => "DpadRight",
                AndroidNative.Views.Keycode.DpadCenter => "DpadCenter",
                AndroidNative.Views.Keycode.Enter => "Enter",
                AndroidNative.Views.Keycode.Back => "Back",
                AndroidNative.Views.Keycode.Escape => "Escape",
                AndroidNative.Views.Keycode.VolumeUp => "VolumeUp",
                AndroidNative.Views.Keycode.VolumeDown => "VolumeDown",
                _ => keyCode.ToString(),
            };
        }

        /// <summary>
        /// 仅映射需要被 WebView 拦截的导航按键，其余返回 null（放行给 WebView 处理）。
        /// </summary>
        public static string GetNavKeyName(AndroidNative.Views.Keycode keyCode)
        {
            return keyCode switch
            {
                AndroidNative.Views.Keycode.DpadUp => "DpadUp",
                AndroidNative.Views.Keycode.DpadDown => "DpadDown",
                AndroidNative.Views.Keycode.DpadLeft => "DpadLeft",
                AndroidNative.Views.Keycode.DpadRight => "DpadRight",
                AndroidNative.Views.Keycode.DpadCenter => "DpadCenter",
                AndroidNative.Views.Keycode.Enter => "Enter",
                AndroidNative.Views.Keycode.Back => "Back",
                AndroidNative.Views.Keycode.Escape => "Escape",
                _ => null,
            };
        }
    }
}
