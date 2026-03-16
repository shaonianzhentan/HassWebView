namespace HassWebView.Core.Events;

/// <summary>
/// Provides data for remote control key events.
/// </summary>
public class RemoteKeyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the original, unprocessed name of the key from the system (e.g., "DpadCenter").
    /// </summary>
    public string SourceKeyName { get; }

    /// <summary>
    /// Gets the normalized, common name of the key (e.g., "Enter", "Back", "Up").
    /// </summary>
    public string KeyName
    {
        get
        {
            return SourceKeyName switch
            {
                "DpadCenter" => "Enter",
                "DpadUp" => "Up",
                "DpadDown" => "Down",
                "DpadLeft" => "Left",
                "DpadRight" => "Right",
                "Escape" => "Back",
                _ => SourceKeyName
            };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteKeyEventArgs"/> class.
    /// </summary>
    /// <param name="sourceKeyName">The original name of the key from the system.</param>
    public RemoteKeyEventArgs(string sourceKeyName)
    { 
        SourceKeyName = sourceKeyName;
    }
}
