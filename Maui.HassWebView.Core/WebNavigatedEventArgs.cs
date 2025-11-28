namespace Maui.HassWebView.Core;

public class WebNavigatedEventArgs : EventArgs
{
    public WebNavigatedEventArgs(string url)
    {
        Url = new Uri(url);
    }

    public Uri Url { get; }
}