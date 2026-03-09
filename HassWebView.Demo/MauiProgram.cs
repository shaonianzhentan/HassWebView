using HassWebView.Core;
using HassWebView.Core.Configuration;
using HassWebView.Core.Views;
using HassWebView.HassApi.Models;
using Microsoft.Extensions.Logging;

namespace HassWebView.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Prepare a variable to pass the server address
            string serverUrl = null;

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configure HttpServer
            builder.UseHttpServer(8125, server =>
            {
                // 1. Immediately store the server address in our variable
                serverUrl = server.BaseUrl;

                // (Your server routing logic remains unchanged)
                server.Get("/", async (req, res) =>
                {
                    await res.Text(DateTime.Now.ToString());
                });

                server.Post("/", async (req, res) =>
                {
                    // When the request arrives, get the correct options instance from the request's service provider
                    var pageOptions = req.Services.GetRequiredService<HassPageOptions>();
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
                            if (data != null)
                            {
                                data.TryGetValue("baseUrl", out baseUrl);
                            }
                            await pageOptions.PlayVideo(payload.Message, baseUrl);
                        }
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });
            });


            // (Your other UseXXX configurations remain unchanged)
            builder
                .UseHassWebView()
                .UseHassPage(options =>
                {
                    // 2. Get the server address from the variable and assign it to PushUrl
                    options.PushUrl = serverUrl;

                    // (Your other page configurations remain unchanged)
                    options.ShowSettingsScreen = () =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Shell.Current.GoToAsync("///MySettingsPage");
                        });
                    };
                })
                .UseImmersiveMode()
                .UseRemoteControl();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            // (The incorrect temporary service container code has been removed)

            return builder.Build();
        }
    }
}
