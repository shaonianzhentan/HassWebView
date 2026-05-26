namespace HassWebView.Core.Models
{
    public class HassInstance
    {
        public string Name { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        
        public string Url => $"http://{HostName}:{Port}";
    }
}