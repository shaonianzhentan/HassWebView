using HassWebView.Core.Services;
using HassWebView.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;

namespace HassWebView.Core.Configuration
{
    public static class HassPageExtensions
    {
        public static MauiAppBuilder UseHassPage(this MauiAppBuilder builder, Action<IServiceProvider, HassPageOptions> configureOptions = null)
        {
            builder.Services.TryAddSingleton<IHassApiService, HassApiService>();

            builder.Services.TryAddSingleton(sp =>
            {
                var options = new HassPageOptions();

                // 修正：实现现在是异步的，并返回一个Task
                options.PlayVideo = async (string url, string? baseUrl, bool external) =>
                {
                    if (external)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            try
                            {
                                #if ANDROID
                                    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                                    intent.SetDataAndType(Android.Net.Uri.Parse(url), "video/*");
                                    intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                                    Android.App.Application.Context.StartActivity(intent);
                                #else
                                    // Launcher 会处理自己的线程调度
                                    _ = Launcher.Default.OpenAsync(new Uri(url));
                                #endif
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[HassPageOptions] 启动外部视频播放器失败: {ex.Message}");
                            }
                        });
                        return;
                    }

                    // 对于内部导航，我们等待整个过程完成
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            // 1. 通过依赖注入创建页面
                            var mediaPage = sp.GetRequiredService<HassMediaPage>();

                            // 2. 设置属性
                            mediaPage.Url = url;

                            if (!string.IsNullOrEmpty(baseUrl))
                            {
                                var uri = new Uri(baseUrl);
                                var host = uri.Host;

                                var config = options.DomainConfigs?
                                    .FirstOrDefault(kvp => host.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                                    .Value;

                                // 假设 WebViewDomainConfig 有一个 Referer 属性，就像之前的代码一样
                                if (config != null)
                                {
                                    // mediaPage.BaseUrl = config.Referer; // The definition of WebViewDomainConfig is not available here, keeping it commented out to avoid compilation errors if Referer doesn't exist.
                                }
                            }

                            // 3. 导航并等待结果
                            await Shell.Current.Navigation.PushModalAsync(mediaPage, true);
                        }
                        catch (Exception ex)
                        {
                           Debug.WriteLine($"[HassPageOptions] 导航到媒体页面时出错: {ex.Message}");
                        }
                    });
                };

                // 允许用户覆盖默认实现
                configureOptions?.Invoke(sp, options);
                return options;
            });

            // 为依赖注入注册页面
            builder.Services.AddTransient<HassPage>();
            builder.Services.AddTransient<HassWebPage>();
            builder.Services.AddTransient<HassMediaPage>();

            return builder;
        }
    }
}
