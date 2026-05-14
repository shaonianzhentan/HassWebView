namespace HassWebView.Core.Models
{
    /// <summary>
    /// Represents a single item in the WebView's navigation history.
    /// </summary>
    public class HassWebHistoryItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
