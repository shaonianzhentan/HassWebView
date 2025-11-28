namespace Maui.HassWebView.Core;

public class WebNavigatingEventArgs : EventArgs
{
    public WebNavigatingEventArgs(string url)
    {
        Url = new Uri(url);
    }

    public Uri Url { get; }

    public bool Cancel { get; set; }
}