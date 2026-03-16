using Android.App;
using Android.OS;
using HassWebView.Core.Services;

namespace HassWebView.Core.Platforms.Android
{
    public class GlobalKeyCallbackManager : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        private readonly KeyService _keyService;

        public GlobalKeyCallbackManager(KeyService keyService)
        {
            _keyService = keyService;
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            var originalCallback = activity.Window.Callback;

            // 如果窗口的回调已经是我们的类型，就不重复包装，避免死循环
            if (originalCallback is KeyCallback)
            {
                return;
            }

            // 使用我们的 KeyCallback 包装原始的回调
            activity.Window.Callback = new KeyCallback(originalCallback, _keyService);
        }

        #region Not Used Lifecycle Methods
        public void OnActivityDestroyed(Activity activity) { }
        public void OnActivityPaused(Activity activity) { }
        public void OnActivityResumed(Activity activity) { }
        public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }
        public void OnActivityStarted(Activity activity) { }
        public void OnActivityStopped(Activity activity) { }
        #endregion
    }
}
