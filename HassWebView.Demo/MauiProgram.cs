using HassWebView.Component;
using HassWebView.Core;
using HassWebView.Core.Configuration;
using HassWebView.HassApi.Models;
using Microsoft.Extensions.Logging;

namespace HassWebView.Demo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseHttpServer(8125, (sp, server) =>
            {
                server.Get("/", async (req, res) =>
                {
                    await res.Text(DateTime.Now.ToString());
                });

                server.Post("/", async (req, res) =>
                {
                    var pageOptions = sp.GetRequiredService<HassPageOptions>();
                    var payload = await req.JsonAsync<NotificationPayload>();

                    if (payload.Title == "url" && payload.Message.StartsWith("http"))
                    {
                        pageOptions.SetWebViewSource?.Invoke(new UrlWebViewSource { Url = payload.Message });
                    }
                    else if (payload.Title == "video")
                    {
                        if (pageOptions.PlayVideo != null)
                        {
                            var baseUrl = string.Empty;
                            var data = payload.Data;
                            // Correctly and safely extract the baseUrl from the dictionary
                            if (data != null && data.TryGetValue("baseUrl", out object baseUrlObject) && baseUrlObject != null)
                            {
                                baseUrl = baseUrlObject.ToString();
                            }
                            await pageOptions.PlayVideo(payload.Message, baseUrl);
                        }
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });
            })
            .UseHassWebView()
            .UseHassPage((sp, options) =>
            {
                options.ShowSettingsScreen = () =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Shell.Current.Navigation.PushModalAsync(new MainPage());
                    });
                };
            })
            .UseImmersiveMode()
            .UseRemoteControl()
            .UseHassComponents();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
