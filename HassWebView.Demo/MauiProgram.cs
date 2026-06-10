using HassWebView.Core;
using HassWebView.Core.Configuration;
using HassWebView.HassApi.Models;
using HassWebView.Component;
using Microsoft.Extensions.Logging;
using HassWebView.Core.Services;

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
            .UseHassWebView()
            .UseHassComponents()
            .UseHassPage<SettingsPage>((sp, options) =>
            {
                var httpServer = sp.GetRequiredService<HttpServer>();
                httpServer.Post("/", async (req, res) =>
                {
                    var payload = await req.JsonAsync<NotificationPayload>();
                    var msg = payload.Message;

                    if (payload.Title == "url" && msg?.StartsWith("http") == true)
                    {
                        options.OpenWebPage?.Invoke(msg);
                    }
                    else if (payload.Title == "config" && msg?.StartsWith("http") == true)
                    {
                        await options.LoadRemoteConfigsAsync(msg);
                    }
                    else if (payload.Title == "video")
                    {
                        if (options.PlayVideo != null)
                        {
                            var baseUrl = string.Empty;
                            var data = payload.Data;
                            if (data != null && data.TryGetValue("baseUrl", out var baseUrlObject) && baseUrlObject != null)
                            {
                                baseUrl = baseUrlObject.ToString() ?? string.Empty;
                            }
                            await options.PlayVideo(payload.Message ?? string.Empty, baseUrl, false);
                        }
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });

                // 打印服务地址方便调试
                Console.WriteLine($"[HttpServer] Listening on: {httpServer.BaseUrl}");
            })
            .UseImmersiveMode()
            .UseRemoteControl();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
