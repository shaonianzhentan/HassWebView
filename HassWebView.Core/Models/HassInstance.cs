namespace HassWebView.Core.Models
{
    public class HassInstance
    {
        public string Name { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        
        public string Url => $"http://{HostName}:{Port}";
    }
}