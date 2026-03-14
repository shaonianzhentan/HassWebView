using System;
using Microsoft.Maui;
using System.Threading.Tasks;

namespace HassWebView.AndroidService.Platforms.Android
{
    /// <summary>
    /// Initializes the ad skipping rules when the application starts.
    /// </summary>
    public class AdSkippingInitializeService : IMauiInitializeService
    {
        private readonly IAdSkippingManager _adSkippingManager;

        public AdSkippingInitializeService(IAdSkippingManager adSkippingManager)
        {
            _adSkippingManager = adSkippingManager;
        }

        public void Initialize(IServiceProvider services)
        {
            // Load rules from the locally saved file on startup.
            _adSkippingManager.LoadRulesFromLocalFileAsync().GetAwaiter().GetResult();
        }
    }
}
