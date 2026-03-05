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
        // 静态资源，生命周期随 App
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, Task> _downloadTasks = new();
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 静态调用：检查、下载并安装腾讯 X5 内核
        /// </summary>
        /// <param name="downloadUrl">X5 内核离线包下载地址 (tbs.apk)</param>
        public static async Task InitializeX5CoreAsync(string downloadUrl)
        {
#if ANDROID
            // 该路径无需额外申请权限，且 X5 SDK 具备读取权限
            var context = Android.App.Application.Context;
            var directory = context.GetExternalFilesDir(null)?.AbsolutePath;
            if (string.IsNullOrEmpty(directory)) return;

            var apkPath = Path.Combine(directory, "tbs.apk");

            // 2. 如果本地不存在 apk，则进入下载流程
            if (!File.Exists(apkPath))
            {
                await EnsureDownloadAsync(downloadUrl, apkPath);
            }

            // 3. 执行 X5 本地安装
            // 确保下载成功后再次检查文件是否存在
            if (File.Exists(apkPath))
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (activity != null)
                {
                    // 参数说明：Context, 类型(0为内核包), APK 路径
                    Com.Tencent.Smtt.Sdk.QbSdk.InstallLocalTbsCore(activity, 0, apkPath);
                    Debug.WriteLine("TencentX5Core加载完成");
                }
            }
#endif
            await Task.CompletedTask;
        }

        private static async Task EnsureDownloadAsync(string url, string path)
        {
            // 任务合并：如果该路径正在下载，则直接返回该任务
            if (_downloadTasks.TryGetValue(path, out var existingTask))
            {
                await existingTask;
                return;
            }

            await _lock.WaitAsync();
            try
            {
                // 双重检查
                if (_downloadTasks.TryGetValue(path, out existingTask))
                {
                    await existingTask;
                    return;
                }

                var downloadTask = DoDownloadInternalAsync(url, path);
                _downloadTasks[path] = downloadTask;
                await downloadTask;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static async Task DoDownloadInternalAsync(string url, string path)
        {
            try
            {
                // 使用流式下载，避免大文件占用过多内存
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception)
            {
                // 下载失败则清理残留，允许下次重试
                if (File.Exists(path)) File.Delete(path);
                throw;
            }
            finally
            {
                // 完成后移除任务引用
                _downloadTasks.TryRemove(path, out _);
            }
        }
    }
}