using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using HassWebView.Core.Events;
using HassWebView.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;

namespace HassWebView.Core.Services;

public class KeyService
{
    private readonly int _longPressTimeout;
    private readonly int _doubleClickTimeout;

    private Timer? _longPressTimer;
    private Timer? _doubleClickTimer;
    private string? _lastSourceKey;
    private int _pressCount = 0;
    private bool _longPressHasFired = false;
    private Timer? _repeatingActionTimer;
    private Action? _repeatingAction;

    public KeyService(int longPressTimeout = 750, int doubleClickTimeout = 300)
    {
        _longPressTimeout = longPressTimeout;
        _doubleClickTimeout = doubleClickTimeout;
    }

    private IKeyHandler? GetCurrentHandler()
    {
        if (!MainThread.IsMainThread)
            return MainThread.InvokeOnMainThreadAsync(GetCurrentHandlerInternal).Result;
        
        return GetCurrentHandlerInternal();
    }
    
    private IKeyHandler? GetCurrentHandlerInternal()
    {
        var navigation = Shell.Current?.Navigation;
        if (navigation == null) return null;
        
        if (navigation.ModalStack.Count > 0)
        {
            return navigation.ModalStack.LastOrDefault() as IKeyHandler;
        }

        return Shell.Current?.CurrentPage as IKeyHandler;
    }

    public void StartRepeatingAction(Action action, int interval = 100)
    {
        StopRepeatingAction();
        _repeatingAction = action;
        _repeatingActionTimer = new Timer(RepeatingActionCallback, null, 0, interval);
    }

    private void RepeatingActionCallback(object? state)
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

    public bool OnPressed(string sourceKeyName)
    {
        var handler = GetCurrentHandler();
        if (handler == null)
        {
            Debug.WriteLine("[KeyService] No active IKeyHandler found.");
            return false;
        }

        var args = new RemoteKeyEventArgs(sourceKeyName);
        var unhandledKeys = handler.GetUnhandledKeys();

        if (unhandledKeys?.Contains(args.KeyName) ?? false)
        {
            Debug.WriteLine($"[KeyService] Key '{args.KeyName}' is unhandled by {handler.GetType().Name} and will be passed to the system.");
            ResetDoubleClickState();
            return false;
        }

        if (_longPressHasFired) return true;

        if (_lastSourceKey != sourceKeyName)
        {
            StopRepeatingAction();
            ResetDoubleClickState();
            _pressCount = 0;
        }

        _lastSourceKey = sourceKeyName;
        _pressCount++;

        _doubleClickTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        if (_pressCount == 1)
        {
            _longPressTimer = new Timer(LongPressTimerCallback, sourceKeyName, _longPressTimeout, Timeout.Infinite);
        }

        return true;
    }

    public bool OnReleased()
    {
        StopRepeatingAction();

        if (_lastSourceKey == null && !_longPressHasFired)
        {
            return false;
        }
        
        var handler = GetCurrentHandler();

        if (_lastSourceKey != null)
        {
            handler?.OnKeyUp(new RemoteKeyEventArgs(_lastSourceKey));
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
            _doubleClickTimer = new Timer(DoubleClickTimerCallback, _lastSourceKey, _doubleClickTimeout, Timeout.Infinite);
        }
        else if (_pressCount >= 2 && _lastSourceKey != null)
        {
            handler?.OnDoubleClick(new RemoteKeyEventArgs(_lastSourceKey));
            ResetDoubleClickState();
        }

        return true;
    }

    private void LongPressTimerCallback(object? state)
    {        
        MainThread.BeginInvokeOnMainThread(() => {
            if (_longPressHasFired) return;
            _longPressHasFired = true;
            
            var handler = GetCurrentHandler();
            var keyName = state as string;
            if (!string.IsNullOrEmpty(keyName))
            {
                handler?.OnLongClick(new RemoteKeyEventArgs(keyName));
            }
        });
    }

    private void DoubleClickTimerCallback(object? state)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            var handler = GetCurrentHandler();
            var keyName = state as string;
            if (!string.IsNullOrEmpty(keyName))
            {
                handler?.OnSingleClick(new RemoteKeyEventArgs(keyName));
            }
            ResetDoubleClickState();
        });
    }

    private void ResetDoubleClickState()
    {
        _pressCount = 0;
        _lastSourceKey = null;
        _doubleClickTimer?.Dispose();
        _doubleClickTimer = null;
        _longPressTimer?.Dispose();
        _longPressTimer = null;
    }
}
