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
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseHassWebView()
                .UseHassPage(options =>
                {
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

            var tempServices = builder.Services.BuildServiceProvider();
            var pageOptions = tempServices.GetRequiredService<HassPageOptions>();

            builder.UseHttpServer(8125, server =>
            {
                pageOptions.PushUrl = server.BaseUrl;

                server.Get("/", async (req, res) =>
                {
                    await res.Text(DateTime.Now.ToString());
                });

                server.Post("/", async (req, res) =>
                {
                    var payload = await req.JsonAsync<NotificationPayload>();

                    if (payload.Title == "url" && payload.Message.StartsWith("http"))
                    {
                        MainThread.BeginInvokeOnMainThread(() => Shell.Current.GoToAsync($"/HassPage?url={Uri.EscapeDataString(payload.Message)}"));
                    }
                    else if (payload.Title == "video")
                    {
                        if (pageOptions.PlayVideo != null)
                        {
                            var baseUrl = string.Empty;
                            var data = payload.Data;
                            if (data != null)
                            {
                                baseUrl = data["baseUrl"];
                            }
                            await pageOptions.PlayVideo(payload.Message, baseUrl);
                        }
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
