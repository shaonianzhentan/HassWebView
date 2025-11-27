
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;

namespace Maui.HassWebView.Core
{
    public static class RemoteControlExtensions
    {
        public static MauiAppBuilder UseRemoteControl(this MauiAppBuilder builder)
        {
            // 1. Register KeyService as a singleton
            builder.Services.AddSingleton<KeyService>();

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android =>
                {
                    // Hook into the main activity's creation
                    android.OnCreate((activity, bundle) =>
                    {
                        var keyService = builder.Services.BuildServiceProvider().GetService<KeyService>();
                        if (keyService == null)
                        {
                            Debug.WriteLine("KeyService not found. Make sure it's registered.");
                            return;
                        }

                        // Hijack the window's callback to intercept key events
                        var window = activity.Window;
                        var originalCallback = window.Callback;
                        window.Callback = new KeyCallback(originalCallback, keyService);
                    });
                });
#elif WINDOWS
                events.AddWindows(windows =>
                {
                    // Hook into the main window's creation
                    windows.OnWindowCreated(window =>
                    {
                        var keyService = builder.Services.BuildServiceProvider().GetService<KeyService>();
                        if (keyService == null)
                        {
                            Debug.WriteLine("KeyService not found. Make sure it's registered.");
                            return;
                        }

                        // Listen for keyboard events
                        window.CoreWindow.KeyDown += (sender, args) =>
                        {
                            // Convert the VirtualKey enum to its string name and pass to the service
                            var keyName = args.VirtualKey.ToString();
                            keyService.OnPressed(keyName);
                            args.Handled = true; // Mark as handled
                        };

                        window.CoreWindow.KeyUp += (sender, args) =>
                        {
                            keyService.OnReleased();
                            args.Handled = true; // Mark as handled
                        };
                    });
                });
#endif
            });

            return builder;
        }

#if ANDROID
        /// <summary>
        /// Custom Window.Callback to intercept key events before they reach the rest of the app.
        /// </summary>
        internal class KeyCallback : Java.Lang.Object, Android.Views.Window.ICallback
        {
            private readonly Android.Views.Window.ICallback _originalCallback;
            private readonly KeyService _keyService;

            public KeyCallback(Android.Views.Window.ICallback originalCallback, KeyService keyService)
            {
                _originalCallback = originalCallback;
                _keyService = keyService;
            }

            public override bool DispatchKeyEvent(Android.Views.KeyEvent e)
            {
                if (e.Action == Android.Views.KeyEventActions.Down)
                {
                    // Convert the Keycode enum to its string name and pass to the service
                    var keyName = e.KeyCode.ToString();
                    _keyService.OnPressed(keyName);
                }
                else if (e.Action == Android.Views.KeyEventActions.Up)
                {
                    _keyService.OnReleased();
                }
                // We handled the event, stop further processing.
                return true; 
            }

            #region Boilerplate: Delegate all other ICallback methods to the original
            public bool DispatchGenericMotionEvent(Android.Views.MotionEvent e) => _originalCallback?.DispatchGenericMotionEvent(e) ?? false;
            public bool DispatchKeyShortcutEvent(Android.Views.KeyEvent e) => _originalCallback?.DispatchKeyShortcutEvent(e) ?? false;
            public bool DispatchPopulateAccessibilityEvent(Android.Views.Accessibility.AccessibilityEvent e) => _originalCallback?.DispatchPopulateAccessibilityEvent(e) ?? false;
            public bool DispatchTouchEvent(Android.Views.MotionEvent e) => _originalCallback?.DispatchTouchEvent(e) ?? false;
            public bool DispatchTrackballEvent(Android.Views.MotionEvent e) => _originalCallback?.DispatchTrackballEvent(e) ?? false;
            public void OnActionModeFinished(Android.Views.ActionMode mode) => _originalCallback?.OnActionModeFinished(mode);
            public void OnActionModeStarted(Android.Views.ActionMode mode) => _originalCallback?.OnActionModeStarted(mode);
            public void OnAttachedToWindow() => _originalCallback?.OnAttachedToWindow();
            public void OnContentChanged() => _originalCallback?.OnContentChanged();
            public bool OnCreatePanelMenu(int featureId, Android.Views.IMenu menu) => _originalCallback?.OnCreatePanelMenu(featureId, menu) ?? false;
            public Android.Views.View OnCreatePanelView(int featureId) => _originalCallback?.OnCreatePanelView(featureId);
            public void OnDetachedFromWindow() => _originalCallback?.OnDetachedFromWindow();
            public bool OnMenuItemSelected(int featureId, Android.Views.IMenuItem item) => _originalCallback?.OnMenuItemSelected(featureId, item) ?? false;
            public bool OnMenuOpened(int featureId, Android.Views.IMenu menu) => _originalCallback?.OnMenuOpened(featureId, menu) ?? false;
            public void OnPanelClosed(int featureId, Android.Views.IMenu menu) => _originalCallback?.OnPanelClosed(featureId, menu);
            public bool OnPreparePanel(int featureId, Android.Views.View view, Android.Views.IMenu menu) => _originalCallback?.OnPreparePanel(featureId, view, menu) ?? false;
            public bool OnSearchRequested() => _originalCallback?.OnSearchRequested() ?? false;
            public bool OnSearchRequested(Android.Views.SearchEvent searchEvent) => _originalCallback?.OnSearchRequested(searchEvent) ?? false;
            public void OnWindowAttributesChanged(Android.Views.WindowManagerLayoutParams attrs) => _originalCallback?.OnWindowAttributesChanged(attrs);
            public void OnWindowFocusChanged(bool hasFocus) => _originalCallback?.OnWindowFocusChanged(hasFocus);
            public Android.Views.ActionMode OnWindowStartingActionMode(Android.Views.ActionMode.ICallback callback) => _originalCallback?.OnWindowStartingActionMode(callback);
            public Android.Views.ActionMode OnWindowStartingActionMode(Android.Views.ActionMode.ICallback callback, int type) => _originalCallback?.OnWindowStartingActionMode(callback, type);
            #endregion
        }
#endif
    }
}
