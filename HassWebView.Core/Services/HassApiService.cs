using HassWebView.HassApi;

namespace HassWebView.Core.Services
{
    public interface IHassApiService
    {
        HassRestApi? Api { get; }
        void Initialize(HassRestApi api);
    }

    public class HassApiService : IHassApiService
    {
        public HassRestApi? Api { get; private set; }

        public void Initialize(HassRestApi api)
        {
            if (Api == null)
            { 
                Api = api;
            }
        }
    }
}