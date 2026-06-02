using Android.Runtime;
using Android.Views;
using HassWebView.Core.Services;

namespace HassWebView.Core.Platforms.Android
{
    public class WebViewKeyListener : Java.Lang.Object, global::Android.Views.View.IOnKeyListener
    {
        public WebViewKeyListener()
        {
        }

        public bool OnKey(global::Android.Views.View? v, [GeneratedEnum] Keycode keyCode, KeyEvent? e)
        {
            var current = IPlatformApplication.Current;
            if (current == null || e == null)
                return false;
                
            var _keyService = current.Services.GetService<KeyService>();
            if (_keyService == null)
                return false;

            string keyName = KeyHelper.GetKeyName(e.KeyCode);

            if (keyName != null)
            {

                // =========================
                // Key Down
                // =========================
                if (e.Action == KeyEventActions.Down)
                {
                    // 👉 抛给全局 KeyService（进入你的长按/双击/单击体系）
                    return _keyService.OnPressed(keyName);
                }

                // =========================
                // Key Up
                // =========================
                if (e.Action == KeyEventActions.Up)
                {
                    return _keyService.OnReleased();
                }
            }

            return false;
        }
    }
}