using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HassWebView.Core.Services
{
    public static class TencentX5Service
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, Task> _downloadTasks = new();
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 检查、下载并安装腾讯 X5 内核，并通过 Action 回调报告进度。
        /// </summary>
        /// <param name="downloadUrl">X5 内核离线包下载地址 (tbs.apk)</param>
        /// <param name="onProgress">用于接收下载进度的回调函数 (0-100)</param>
        /// <returns>返回 true 表示内核安装成功，返回 false 表示失败。</returns>
        public static async Task<bool> InitializeX5CoreAsync(string downloadUrl, Action<int> onProgress = null)
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var directory = context.GetExternalFilesDir(null)?.AbsolutePath;
                if (string.IsNullOrEmpty(directory)) return false;

                var apkPath = Path.Combine(directory, "tbs.apk");

                if (!File.Exists(apkPath))
                {
                    await EnsureDownloadAsync(downloadUrl, apkPath, onProgress);
                }

                if (File.Exists(apkPath))
                {
                    var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                    if (activity != null)
                    {
                        // 参数说明：Context, 类型(0为内核包), APK 路径
                        Com.Tencent.Smtt.Sdk.QbSdk.InstallLocalTbsCore(activity, 0, apkPath);
                        Debug.WriteLine("TencentX5Core installation command issued.");
                        return true; // 安装指令已发出
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize X5 core: {ex.Message}");
                return false; // 出现任何异常均视为失败
            }
#endif
            return false; // 非 Android 平台直接返回失败
        }

        private static async Task EnsureDownloadAsync(string url, string path, Action<int> onProgress)
        {
            if (_downloadTasks.TryGetValue(path, out var existingTask))
            {
                await existingTask;
                return;
            }

            await _lock.WaitAsync();
            try
            {
                if (_downloadTasks.TryGetValue(path, out existingTask))
                {
                    await existingTask;
                    return;
                }

                var downloadTask = DoDownloadInternalAsync(url, path, onProgress);
                _downloadTasks[path] = downloadTask;
                await downloadTask;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static async Task DoDownloadInternalAsync(string url, string path, Action<int> onProgress)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                
                var buffer = new byte[81920]; // 80KB buffer
                long totalBytesRead = 0;
                int bytesRead;
                int lastPercentage = -1;
                
                // 报告一次0%的进度，让前端立即知道下载已开始
                onProgress?.Invoke(0);

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    if (totalBytes > 0)
                    {
                        var percentage = (int)((double)totalBytesRead * 100 / totalBytes);
                        if (percentage > lastPercentage)
                        {
                            lastPercentage = percentage;
                            // 直接调用 Action<int> 传回进度值
                            onProgress?.Invoke(percentage);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (File.Exists(path)) File.Delete(path);
                throw; // 向上抛出异常，由 InitializeX5CoreAsync 捕获并返回 false
            }
            finally
            {
                _downloadTasks.TryRemove(path, out _);
            }
        }
    }
}
