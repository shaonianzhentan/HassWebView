using System.Collections.Generic;

namespace HassWebView.Core.Models
{
    /// <summary>
    /// Represents a snapshot of the WebView's back-forward list.
    /// </summary>
    public class WebViewBackForwardList
    {
        public IList<WebViewHistoryItem> History { get; set; }
        public int CurrentIndex { get; set; }
    }
}
