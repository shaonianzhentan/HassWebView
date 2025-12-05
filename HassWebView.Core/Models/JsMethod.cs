using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HassWebView.Core.Models
{
    public class JsMethod
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("arguments")]
        public object[] Arguments { get; set; }
    }
}