using YamlDotNet.Serialization;

namespace HassWebView.Core.Configuration
{
    public class WebViewDomainConfig
    {
        [YamlMember(Alias = "ua")]
        public string UserAgent { get; set; } = string.Empty;

        [YamlMember(Alias = "referer")]
        public string Referer { get; set; } = string.Empty;

        [YamlMember(Alias = "css")]
        public string Css { get; set; } = string.Empty;

        [YamlMember(Alias = "js")]
        public string Js { get; set; } = string.Empty;
    }
}
