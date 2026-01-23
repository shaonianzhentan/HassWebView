using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core;

public static class RemoteControlBehavior
{
    // This dictionary will hold the state for each view the behavior is attached to.
    private static readonly Dictionary<HassAuthenticatedView, RemoteControlState> _states = new();

    #region IsEnabled Attached Property

    public static readonly BindableProperty IsEnabledProperty =
        BindableProperty.CreateAttached(
            "IsEnabled",
            typeof(bool),
            typeof(RemoteControlBehavior),
            false, // Default value is false
            propertyChanged: OnIsEnabledChanged);

    public static bool GetIsEnabled(BindableObject view) => (bool)view.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(BindableObject view, bool value) => view.SetValue(IsEnabledProperty, value);

    #endregion

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not HassAuthenticatedView view) {
            Debug.WriteLine("[RemoteControlBehavior] Error: Behavior can only be attached to a HassAuthenticatedView.");
            return;
        }

        bool isEnabled = (bool)newValue;

        if (isEnabled)
        {
            // Avoid attaching more than once
            if (_states.ContainsKey(view)) return;

            Debug.WriteLine("[RemoteControlBehavior] Enabling for a view.");
            var state = new RemoteControlState(view);
            state.Attach();
            _states[view] = state;
        }
        else
        {
            // Detach if it was previously enabled
            if (_states.TryGetValue(view, out var state))
            {
                Debug.WriteLine("[RemoteControlBehavior] Disabling for a view.");
                state.Detach();
                _states.Remove(view);
            }
        }
    }

    /// <summary>
    /// Internal class to manage the state of the remote control for a single view.
    /// </summary>
    private class RemoteControlState
    {
        private readonly HassAuthenticatedView _view;
        private readonly HassWebView _webView;
        private readonly KeyService _keyService;
        private readonly CursorControl _cursorControl;
        private readonly Frame _cursor;
        private Layout _parentLayout;

        public RemoteControlState(HassAuthenticatedView view)
        {
            _view = view;
            _webView = view.FindByName<HassWebView>("webView");

            // Resolve services from the global service provider
            _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();

            // Create the cursor UI element programmatically
            _cursor = new Frame
            {
                BackgroundColor = Color.FromArgb("#03a9f4"),
                BorderColor = Color.FromArgb("#0277bd"),
                CornerRadius = 10,
                WidthRequest = 20,
                HeightRequest = 20,
                IsVisible = true,
                Opacity = 0.8,
                ZIndex = 999 // Ensure cursor is on top
            };

            // The parent finding logic runs in the Attach method
            _cursorControl = null; 
        }

        public void Attach()
        {   
            // We need to wait until the view is part of the layout
            _view.ParentChanged += OnParentChanged;
        }

        private void OnParentChanged(object sender, EventArgs e)
        { 
            if (_view.Parent is not Layout parentLayout) return;

            // This logic runs only once when the parent is found
            _view.ParentChanged -= OnParentChanged; // Unsubscribe to avoid re-running

            _parentLayout = parentLayout;

            // Initialize and add the cursor
            if (_parentLayout is AbsoluteLayout absLayout) 
            {
                AbsoluteLayout.SetLayoutBounds(_cursor, new Rect(100, 100, 20, 20));
                AbsoluteLayout.SetLayoutFlags(_cursor, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
                absLayout.Children.Add(_cursor);
            }
            else if (_parentLayout is Grid grid) 
            {
                _cursor.HorizontalOptions = LayoutOptions.Start;
                _cursor.VerticalOptions = LayoutOptions.Start;
                _cursor.Margin = new Thickness(100, 100, 0, 0);
                grid.Children.Add(_cursor);
            }

            // Now we can create the cursor controller
            _cursorControl = new CursorControl(_cursor, _parentLayout, _webView);
            
            // Subscribe to key events
            _keyService.SingleClick += OnSingleClick;
            _keyService.DoubleClick += OnDoubleClick;
            _keyService.LongClick += OnLongClick;
        }

        public void Detach()
        {
            _view.ParentChanged -= OnParentChanged;
            if (_parentLayout != null && _parentLayout.Children.Contains(_cursor)){
                 _parentLayout.Children.Remove(_cursor);
            }
            _keyService.SingleClick -= OnSingleClick;
            _keyService.DoubleClick -= OnDoubleClick;
            _keyService.LongClick -= OnLongClick;
        }

        // --- Key event handlers that control the cursor ---
        private void OnSingleClick(object s, RemoteKeyEventArgs e) => HandleKey(e.KeyName, "Single");
        private void OnDoubleClick(object s, RemoteKeyEventArgs e) => HandleKey(e.KeyName, "Double");
        private void OnLongClick(object s, RemoteKeyEventArgs e) => HandleKey(e.KeyName, "Long");

        private void HandleKey(string keyName, string clickType)
        {
            if (_cursorControl == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                 switch (clickType)
                 {
                    case "Single":
                        switch (keyName) {
                            case "Enter": case "DpadCenter": _cursorControl.Click(); break;
                            case "Up": case "DpadUp": _cursorControl.MoveUpBy(); break;
                            case "Down": case "DpadDown": _cursorControl.MoveDownBy(); break;
                            case "Left": case "DpadLeft": _cursorControl.MoveLeftBy(); break;
                            case "Right": case "DpadRight": _cursorControl.MoveRightBy(); break;
                        }
                        break;
                    case "Double":
                         switch (keyName) {
                            case "Enter": case "DpadCenter": _cursorControl.DoubleClick(); break;
                            case "Up": case "DpadUp": _cursorControl.SlideUp(); break;
                            case "Down": case "DpadDown": _cursorControl.SlideDown(); break;
                            case "Left": case "DpadLeft": _cursorControl.SlideLeft(); break;
                            case "Right": case "DpadRight": _cursorControl.SlideRight(); break;
                        }
                        break;
                    case "Long":
                        int interval = 100;
                        switch (keyName) {
                           case "Up": case "DpadUp": _keyService.StartRepeatingAction(() => _cursorControl.MoveUpBy(), interval); break;
                           case "Down": case "DpadDown": _keyService.StartRepeatingAction(() => _cursorControl.MoveDownBy(), interval); break;
                           case "Left": case "DpadLeft": _keyService.StartRepeatingAction(() => _cursorControl.MoveLeftBy(), interval); break;
                           case "Right": case "DpadRight": _keyService.StartRepeatingAction(() => _cursorControl.MoveRightBy(), interval); break;
                        }
                        break;
                 }
            });
        }
    }
}
