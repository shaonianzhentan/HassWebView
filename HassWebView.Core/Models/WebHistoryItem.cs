namespace HassWebView.Core.Models
{
    /// <summary>
    /// Represents a single item in the WebView's navigation history.
    /// </summary>
    public class WebHistoryItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
