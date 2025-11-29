namespace Maui.HassWebView.Core;

/// <summary>
/// Provides data for the key-related events in KeyService.
/// </summary>
public class KeyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the key that was pressed.
    /// </summary>
    public string KeyName { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the key event has been handled.
    /// Set this to <c>false</c> in an event handler to indicate that you did not handle the event,
    /// which may allow other handlers or default behaviors to process it.
    /// The default is <c>true</c>, meaning the event is considered handled by default.
    /// </summary>
    public bool Handled { get; set; }

    public KeyEventArgs(string keyName)
    {
        KeyName = keyName;
        Handled = true; // Default to handled
    }
}
