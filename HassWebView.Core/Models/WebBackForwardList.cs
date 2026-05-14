using System.Collections.Generic;

namespace HassWebView.Core.Models
{
    /// <summary>
    /// Represents a snapshot of the WebView's back-forward list.
    /// </summary>
    public class HassWebBackForwardList
    {
        public IList<HassWebHistoryItem> History { get; set; }
        public int CurrentIndex { get; set; }
    }
}
