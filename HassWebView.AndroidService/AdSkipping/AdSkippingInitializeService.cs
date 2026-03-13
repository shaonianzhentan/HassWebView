using Microsoft.Maui.Hosting;

namespace HassWebView.AndroidService.AdSkipping
{
    internal class AdSkippingInitializeService : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {
            Task.Run(async () =>
            {
                try
                {
                    var rulesManager = services.GetRequiredService<IAdRulesManager>();
                    await rulesManager.LoadRulesFromLocalFileAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AdSkipping] Error initializing ad rules: {ex.Message}");
                }
            });
        }
    }
}
