using HassWebView.Core;
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
                .UseHassWebView(options =>
                {
                    options.ShowSettingsScreen = () =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Shell.Current.GoToAsync("///MySettingsPage");
                        });
                    };

                    options.PlayVideo = (url) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Shell.Current.GoToAsync($"/{nameof(HassPage)}/{nameof(HassMediaPage)}?Url={Uri.EscapeDataString(url)}");
                        });
                    };
                })
                .UseImmersiveMode()
                .UseRemoteControl();

            var tempServices = builder.Services.BuildServiceProvider();
            var webViewOptions = tempServices.GetRequiredService<HassWebViewOptions>();

            builder.UseHttpServer(8125, server =>
            {
                webViewOptions.PushUrl = server.BaseUrl;

                server.Get("/", async (req, res) =>
                {
                    await res.Text(DateTime.Now.ToString());
                });

                server.Post("/", async (req, res) =>
                {
                    var payload = await req.JsonAsync<NotificationPayload>();

                    if (payload.Title == "url" && payload.Message.StartsWith("http"))
                    {
                        MainThread.BeginInvokeOnMainThread(() => Shell.Current.GoToAsync($"/{nameof(HassPage)}?url={Uri.EscapeDataString(payload.Message)}"));
                    }
                    else if (payload.Title == "video")
                    {
                        MainThread.BeginInvokeOnMainThread(() => Shell.Current.GoToAsync($"/{nameof(HassMediaPage)}?url={Uri.EscapeDataString(payload.Message)}"));
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder.Build();
        }
    }
}
