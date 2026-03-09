using YamlDotNet.Serialization;

namespace HassWebView.Core.Configuration
{
    public class WebViewDomainConfig
    {
        [YamlMember(Alias = "ua")]
        public string UserAgent { get; set; }

        [YamlMember(Alias = "referer")]
        public string Referer { get; set; }

        [YamlMember(Alias = "css")]
        public string Css { get; set; }

        [YamlMember(Alias = "js")]
        public string Js { get; set; }
    }
}
