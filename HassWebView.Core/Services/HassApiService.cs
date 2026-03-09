
using HassWebView.HassApi;

namespace HassWebView.Core.Services
{
    /// <summary>
    /// An interface to access the shared HassRestApi instance.
    /// </summary>
    public interface IHassApiService
    {
        HassRestApi Api { get; }
        void Initialize(HassRestApi api);
    }

    /// <summary>
    /// A singleton service to hold the initialized HassRestApi instance.
    /// </summary>
    public class HassApiService : IHassApiService
    {
        public HassRestApi Api { get; private set; }

        public void Initialize(HassRestApi api)
        {
            // Allow initialization only once to avoid race conditions or accidental overwrites
            if (Api == null)
            { 
                Api = api;
            }
        }
    }
}
