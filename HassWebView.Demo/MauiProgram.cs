using HassWebView.Core;
using HassWebView.Core.Configuration;
using HassWebView.HassApi.Models;
using HassWebView.Component;
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

                    var msg = payload.Message;
                    if (payload.Title == "url" && msg?.StartsWith("http") == true)
                    {
                        pageOptions.OpenWebPage?.Invoke(msg);
                    }
                    else if (payload.Title == "config" && msg?.StartsWith("http") == true)
                    {
                        // 加载远程配置
                        await pageOptions.LoadRemoteConfigsAsync(msg);
                    }
                    else if (payload.Title == "video")
                    {
                        if (pageOptions.PlayVideo != null)
                        {
                            var baseUrl = string.Empty;
                            var data = payload.Data;
                            if (data != null && data.TryGetValue("baseUrl", out object? baseUrlObject) && baseUrlObject != null)
                            {
                                baseUrl = baseUrlObject.ToString() ?? string.Empty;
                            }
                            // 保留修正：添加 'external: false' 参数以匹配委托签名
                            await pageOptions.PlayVideo(payload.Message ?? string.Empty, baseUrl, false);
                        }
                    }
                    await res.Text("", System.Net.HttpStatusCode.Created);
                });
            })
            .UseHassWebView()
            .UseHassComponents()
            .UseHassPage<SettingsPage>()
            .UseImmersiveMode()
            .UseRemoteControl();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
