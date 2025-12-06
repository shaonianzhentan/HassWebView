using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using HassWebView.Core.Events;

namespace HassWebView.Core.Services;

public class KeyService
{
    private IKeyHandler _currentHandler;
    private bool _isEnabled = false; // Disabled by default

    private readonly int _longPressTimeout;
    private readonly int _doubleClickTimeout;

    private Timer _longPressTimer;
    private Timer _doubleClickTimer;
    private string _lastKey;
    private int _pressCount = 0;
    private bool _longPressHasFired = false;
    private Timer _repeatingActionTimer;
    private Action _repeatingAction;

    public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300)
    {
        _longPressTimeout = longPressTimeout;
        _doubleClickTimeout = doubleClickTimeout;
    }

    public void Enable() 
    {
        Debug.WriteLine("[KeyService] Enabled");
        _isEnabled = true; 
    }

    public void Disable() 
    {
        Debug.WriteLine("[KeyService] Disabled");
        _isEnabled = false; 
    }

    public void Register(IKeyHandler handler)
    {
        Debug.WriteLine($"[KeyService] Registering handler: {handler.GetType().Name}");
        _currentHandler = handler;
    }

    public void Unregister()
    {
        Debug.WriteLine("[KeyService] Unregistering handler.");
        _currentHandler = null;
        ResetDoubleClickState();
        StopRepeatingAction();
    }

    public void StartRepeatingAction(Action action, int interval = 100)
    {
        if (!_isEnabled || _currentHandler == null) return;
        StopRepeatingAction();
        _repeatingAction = action;
        _repeatingActionTimer = new Timer(RepeatingActionCallback, null, 0, interval);
    }

    private void RepeatingActionCallback(object state)
    {
        MainThread.BeginInvokeOnMainThread(() => _repeatingAction?.Invoke());
    }

    public void StopRepeatingAction()
    {
        _repeatingActionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _repeatingActionTimer?.Dispose();
        _repeatingActionTimer = null;
        _repeatingAction = null;
    }

    public bool OnPressed(string keyName)
    {
        if (!_isEnabled || _currentHandler == null)
        {
            return false;
        }

        var args = new RemoteKeyEventArgs(keyName);
        bool handled = _currentHandler.OnKeyDown(this, args);

        if (!handled)
        {
            ResetDoubleClickState();
            return false;
        }

        if (_longPressHasFired) return true;

        if (_lastKey != keyName)
        {
            StopRepeatingAction();
            ResetDoubleClickState();
            _pressCount = 0;
        }

        _lastKey = keyName;
        _pressCount++;

        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            _longPressTimer = new Timer(LongPressTimerCallback, keyName, _longPressTimeout, Timeout.Infinite);
        }

        return true;
    }

    public bool OnReleased()
    {
        if (!_isEnabled || _currentHandler == null)
        {
             return false;
        }

        if (_lastKey == null && !_longPressHasFired)
        {
            return false;
        }
        
        if (_lastKey != null)
        {
            _currentHandler.OnKeyUp(this, new RemoteKeyEventArgs(_lastKey));
        }

        if (_longPressHasFired)
        {
            _longPressHasFired = false;
            ResetDoubleClickState();
            return true;
        }

        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            _doubleClickTimer = new Timer(DoubleClickTimerCallback, _lastKey, _doubleClickTimeout, Timeout.Infinite);
        }
        else if (_pressCount >= 2)
        {
            _currentHandler.OnDoubleClick(this, new RemoteKeyEventArgs(_lastKey));
            ResetDoubleClickState();
        }

        return true;
    }

    private void LongPressTimerCallback(object state)
    {
        if (!_isEnabled || _longPressHasFired) return;
        _longPressHasFired = true;
        _currentHandler?.OnLongClick(this, new RemoteKeyEventArgs((string)state));
    }

    private void DoubleClickTimerCallback(object state)
    {
        if (!_isEnabled) return;
        _currentHandler?.OnSingleClick(this, new RemoteKeyEventArgs((string)state));
        ResetDoubleClickState();
    }

    private void ResetDoubleClickState()
    {
        _pressCount = 0;
        _lastKey = null;
        _doubleClickTimer?.Dispose();
        _doubleClickTimer = null;
        _longPressTimer?.Dispose();
        _longPressTimer = null;
    }
}
