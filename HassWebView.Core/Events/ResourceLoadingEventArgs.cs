using System.ComponentModel;

namespace HassWebView.Core.Events;

public class ResourceLoadingEventArgs : CancelEventArgs
{
    public string Url { get; }

    public ResourceLoadingEventArgs(string url)
    {
        Url = url;
    }
}
