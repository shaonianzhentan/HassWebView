using AndroidNative = Android;

namespace Maui.HassWebView.Core.Platforms.Android
{
    /// <summary>
    /// Custom Window.Callback to intercept key events before they reach the rest of the app.
    /// </summary>
    public class KeyCallback : Java.Lang.Object, AndroidNative.Views.Window.ICallback
    {
        private readonly AndroidNative.Views.Window.ICallback _originalCallback;
        private readonly KeyService _keyService;

        public KeyCallback(AndroidNative.Views.Window.ICallback originalCallback, KeyService keyService)
        {
            _originalCallback = originalCallback;
            _keyService = keyService;
        }

        public bool DispatchKeyEvent(AndroidNative.Views.KeyEvent? e)
        {
            if (e == null)
            {
                return _originalCallback?.DispatchKeyEvent(e) ?? false;
            }

            // Directly pass the key event to the KeyService
            if (e.Action == AndroidNative.Views.KeyEventActions.Down)
            {
                var keyName = e.KeyCode.ToString();
                _keyService.OnPressed(keyName);
                return true; // We've handled the event.
            }
            else if (e.Action == AndroidNative.Views.KeyEventActions.Up)
            { 
                _keyService.OnReleased();
                return true; // We've handled the event.
            }

            // For any other action, let the original callback handle it.
            return _originalCallback?.DispatchKeyEvent(e) ?? false;
        }

        #region Boilerplate: Delegate all other ICallback methods to the original
        public bool DispatchGenericMotionEvent(AndroidNative.Views.MotionEvent? e) => _originalCallback?.DispatchGenericMotionEvent(e) ?? false;
        public bool DispatchKeyShortcutEvent(AndroidNative.Views.KeyEvent? e) => _originalCallback?.DispatchKeyShortcutEvent(e) ?? false;
        public bool DispatchPopulateAccessibilityEvent(AndroidNative.Views.Accessibility.AccessibilityEvent? e) => _originalCallback?.DispatchPopulateAccessibilityEvent(e) ?? false;
        public bool DispatchTouchEvent(AndroidNative.Views.MotionEvent? e) => _originalCallback?.DispatchTouchEvent(e) ?? false;
        public bool DispatchTrackballEvent(AndroidNative.Views.MotionEvent? e) => _originalCallback?.DispatchTrackballEvent(e) ?? false;
        public void OnActionModeFinished(AndroidNative.Views.ActionMode? mode) => _originalCallback?.OnActionModeFinished(mode);
        public void OnActionModeStarted(AndroidNative.Views.ActionMode? mode) => _originalCallback?.OnActionModeStarted(mode);
        public void OnAttachedToWindow() => _originalCallback?.OnAttachedToWindow();
        public void OnContentChanged() => _originalCallback?.OnContentChanged();
        public bool OnCreatePanelMenu(int featureId, AndroidNative.Views.IMenu? menu) => _originalCallback?.OnCreatePanelMenu(featureId, menu) ?? false;
        public AndroidNative.Views.View? OnCreatePanelView(int featureId) => _originalCallback?.OnCreatePanelView(featureId);
        public void OnDetachedFromWindow() => _originalCallback?.OnDetachedFromWindow();
        public bool OnMenuItemSelected(int featureId, AndroidNative.Views.IMenuItem? item) => _originalCallback?.OnMenuItemSelected(featureId, item) ?? false;
        public bool OnMenuOpened(int featureId, AndroidNative.Views.IMenu? menu) => _originalCallback?.OnMenuOpened(featureId, menu) ?? false;
        public void OnPanelClosed(int featureId, AndroidNative.Views.IMenu? menu) => _originalCallback?.OnPanelClosed(featureId, menu);
        public bool OnPreparePanel(int featureId, AndroidNative.Views.View? view, AndroidNative.Views.IMenu? menu) => _originalCallback?.OnPreparePanel(featureId, view, menu) ?? false;
        public bool OnSearchRequested() => _originalCallback?.OnSearchRequested() ?? false;
        public bool OnSearchRequested(AndroidNative.Views.SearchEvent? searchEvent) => _originalCallback?.OnSearchRequested(searchEvent) ?? false;
        public void OnWindowAttributesChanged(AndroidNative.Views.WindowManagerLayoutParams? attrs) => _originalCallback?.OnWindowAttributesChanged(attrs);
        public void OnWindowFocusChanged(bool hasFocus) => _originalCallback?.OnWindowFocusChanged(hasFocus);
        public AndroidNative.Views.ActionMode? OnWindowStartingActionMode(AndroidNative.Views.ActionMode.ICallback? callback) => _originalCallback?.OnWindowStartingActionMode(callback);
        public AndroidNative.Views.ActionMode? OnWindowStartingActionMode(AndroidNative.Views.ActionMode.ICallback? callback, AndroidNative.Views.ActionModeType type) => _originalCallback?.OnWindowStartingActionMode(callback, type);
        #endregion
    }
}
