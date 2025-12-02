using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using HassWebView.Core.Events;

namespace HassWebView.Core;

public class KeyService
{
    // --- NEW: A synchronous event fired immediately on key press ---
    public event Action<RemoteKeyEventArgs> KeyDown;

    public event Action<RemoteKeyEventArgs> SingleClick;
    public event Action<RemoteKeyEventArgs> DoubleClick;
    public event Action<RemoteKeyEventArgs> LongClick;
    public event Action<RemoteKeyEventArgs> KeyUp; 

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

    public void StartRepeatingAction(Action action, int interval = 100)
    {
        StopRepeatingAction();
        _repeatingAction = action;
        _repeatingActionTimer = new Timer(RepeatingActionCallback, null, 0, interval);
    }

    private void RepeatingActionCallback(object state)
    {
        MainThread.BeginInvokeOnMainThread(() => _repeatingAction?.Invoke());
    }
    
    private void StopRepeatingAction()
    {
        _repeatingActionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _repeatingActionTimer?.Dispose();
        _repeatingActionTimer = null;
        _repeatingAction = null;
    }

    // --- MODIFIED: OnPressed now returns a boolean and fires the KeyDown event first ---
    public bool OnPressed(string keyName)
    {
        // 1. Create args and immediately fire the synchronous KeyDown event.
        var args = new RemoteKeyEventArgs(keyName);
        KeyDown?.Invoke(args);

        // 2. Check if the user has decided to let the system handle it.
        if (!args.Handled)
        {
            // If Handled is false, we stop all further processing in this service...
            ResetDoubleClickState(); 
            // ...and tell the platform layer (e.g., RemoteControlExtensions) not to intercept the event.
            return false; 
        }

        // 3. If Handled is true (default), proceed with gesture detection.
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
        
        // Tell the platform layer to intercept the event because we are handling it.
        return true;
    }

    public void OnReleased()
    {
        StopRepeatingAction();

        if (_lastKey != null)
        {
            KeyUp?.Invoke(new RemoteKeyEventArgs(_lastKey));
        }

        if (_longPressHasFired)
        { 
            _longPressHasFired = false;
            ResetDoubleClickState();
            return;
        }

        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            _doubleClickTimer = new Timer(DoubleClickTimerCallback, _lastKey, _doubleClickTimeout, Timeout.Infinite);
        }
        else if (_pressCount >= 2)
        {
            Debug.WriteLine("DoubleClick detected");
            DoubleClick?.Invoke(new RemoteKeyEventArgs(_lastKey));
            ResetDoubleClickState();
        }
    }

    private void LongPressTimerCallback(object state)
    { 
        Debug.WriteLine("LongClick detected");
        if (_longPressHasFired) return;
        _longPressHasFired = true;
        LongClick?.Invoke(new RemoteKeyEventArgs((string)state));
        // We don't ResetDoubleClickState here anymore to allow KeyUp to know about the state.
    }

    private void DoubleClickTimerCallback(object state)
    {
        Debug.WriteLine("SingleClick detected");
        SingleClick?.Invoke(new RemoteKeyEventArgs((string)state));
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
