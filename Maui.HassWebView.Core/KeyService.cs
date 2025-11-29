using System.Diagnostics;

namespace Maui.HassWebView.Core;

public class KeyService
{
    // Events using the new KeyEventArgs
    public event Action<KeyEventArgs> SingleClick;
    public event Action<KeyEventArgs> DoubleClick;
    public event Action<KeyEventArgs> LongClick;

    private readonly int _longPressTimeout;
    private readonly int _doubleClickTimeout;

    private System.Threading.Timer _longPressTimer;
    private System.Threading.Timer _doubleClickTimer;
    private string _lastKey;
    private int _pressCount = 0;

    // Constructor without downInterval
    public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300)
    {
        _longPressTimeout = longPressTimeout;
        _doubleClickTimeout = doubleClickTimeout;
    }

    public void OnPressed(string keyName)
    {
        // If a different key is pressed, reset everything.
        if (_lastKey != keyName)
        {
            ResetDoubleClickState();
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
        }
    }

    public void OnReleased()
    {
        // A release always cancels a potential long press.
        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            // This could be a single click. Start the double-click timer to see if another press follows.
            _doubleClickTimer = new System.Threading.Timer(DoubleClickTimerCallback, _lastKey, _doubleClickTimeout, Timeout.Infinite);
        }
        else if (_pressCount >= 2)
        {
            // This is a double click.
            Debug.WriteLine("DoubleClick detected");
            DoubleClick?.Invoke(new KeyEventArgs(_lastKey));
            ResetDoubleClickState();
        }
    }

    private void LongPressTimerCallback(object state)
    { 
        Debug.WriteLine("LongClick detected");
        LongClick?.Invoke(new KeyEventArgs((string)state));
        // A long press is a definitive event, so reset the click tracking.
        ResetDoubleClickState();
    }

    private void DoubleClickTimerCallback(object state)
    {
        // If this timer fires, it means no second click occurred in time, so it's a single click.
        Debug.WriteLine("SingleClick detected");
        SingleClick?.Invoke(new KeyEventArgs((string)state));
        ResetDoubleClickState();
    }

    private void ResetDoubleClickState()
    {
        _pressCount = 0;
        _lastKey = null;
        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _longPressTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }
}
