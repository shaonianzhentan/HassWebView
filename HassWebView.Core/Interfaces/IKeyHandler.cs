using HassWebView.Core.Events;
using System;

namespace HassWebView.Core.Interfaces;

public interface IKeyHandler
{
    /// <summary>
    /// Returns a list of key names that this handler wishes to ignore.
    /// These keys will be passed up to the system for default handling (e.g., volume control).
    /// </summary>
    /// <returns>An array of key names to be ignored.</returns>
    string[] GetUnhandledKeys() => Array.Empty<string>();

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    void OnKeyUp(RemoteKeyEventArgs args) { }

    /// <summary>
    /// Called on a single key click.
    /// </summary>
    void OnSingleClick(RemoteKeyEventArgs args) { }

    /// <summary>
    /// Called on a double key click.
    /// </summary>
    void OnDoubleClick(RemoteKeyEventArgs args) { }

    /// <summary>
    /// Called on a long key press.
    /// </summary>
    void OnLongClick(RemoteKeyEventArgs args) { }
}
