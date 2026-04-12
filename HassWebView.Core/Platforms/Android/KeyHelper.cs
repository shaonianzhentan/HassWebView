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
    }
}
