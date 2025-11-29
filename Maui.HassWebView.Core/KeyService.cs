using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace Maui.HassWebView.Core;

public class KeyService
{
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

    // --- REPEAT ACTION ---
    private Timer _repeatingActionTimer;
    private Action _repeatingAction;

    public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300)
    {
        _longPressTimeout = longPressTimeout;
        _doubleClickTimeout = doubleClickTimeout;
    }

    // --- ADDED: Public method to start the repeating action ---
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

    public void OnPressed(string keyName)
    {
        if (_longPressHasFired) return;

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
        _longPressHasFired = true;
        LongClick?.Invoke(new RemoteKeyEventArgs((string)state));
        ResetDoubleClickState();
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
        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }
}
