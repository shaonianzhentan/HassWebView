using System.Diagnostics;

namespace Maui.HassWebView.Core;

public class KeyService
{
    // Events using the new KeyEventArgs
    public event Action<KeyEventArgs> SingleClick;
    public event Action<KeyEventArgs> DoubleClick;
    public event Action<KeyEventArgs> LongClick;
    public event Action<KeyEventArgs> Down;

    private readonly int _longPressTimeout;
    private readonly int _doubleClickTimeout;
    private readonly int _downInterval;

    private System.Threading.Timer _longPressTimer;
    private System.Threading.Timer _doubleClickTimer;
    private System.Threading.Timer _downTimer;
    private string _lastKey;
    private int _pressCount = 0;

    public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300, int downInterval = 100)
    {
        _longPressTimeout = longPressTimeout;
        _doubleClickTimeout = doubleClickTimeout;
        _downInterval = downInterval;
    }

    public void OnPressed(string keyName)
    {
        // If a different key is pressed, reset everything.
        if (_lastKey != keyName)
        {
            ResetDoubleClick();
            _pressCount = 0;
        }
        
        _lastKey = keyName;
        _pressCount++;

        // Stop the double-click timer since a new press has occurred.
        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            // Start the long press timer on the first press.
            _longPressTimer = new System.Threading.Timer(LongPressTimerCallback, keyName, _longPressTimeout, Timeout.Infinite);
            // Start the down timer.
            _downTimer = new System.Threading.Timer(DownTimerCallback, keyName, 0, _downInterval);
        }
    }

    public void OnReleased()
    {
        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _downTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            // This could be a single click, start the double-click timer to check.
            _doubleClickTimer = new System.Threading.Timer(DoubleClickTimerCallback, _lastKey, _doubleClickTimeout, Timeout.Infinite);
        }
        else if (_pressCount >= 2)
        {
            // This is a double click.
            Debug.WriteLine("DoubleClick detected");
            DoubleClick?.Invoke(new KeyEventArgs(_lastKey));
            ResetDoubleClick();
        }
    }

    private void DownTimerCallback(object state)
    {
        string keyName = (string)state;
        Debug.WriteLine($"Down event for {keyName}");
        Down?.Invoke(new KeyEventArgs(keyName));
    }

    private void LongPressTimerCallback(object state)
    { 
        _downTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        Debug.WriteLine("LongClick detected");
        LongClick?.Invoke(new KeyEventArgs((string)state));
        ResetDoubleClick();
    }

    private void DoubleClickTimerCallback(object state)
    {
        // If the timer fires, it means no second click occurred in time.
        Debug.WriteLine("SingleClick detected");
        SingleClick?.Invoke(new KeyEventArgs((string)state));
        ResetDoubleClick();
    }

    private void ResetDoubleClick()
    {
        _pressCount = 0;
        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }
}
