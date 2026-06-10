using System;
using System.Runtime.InteropServices;

namespace HassWebView.Core.Configuration
{
    /// <summary>
    /// HassWebView 控件配置选项
    /// </summary>
    public class HassWebViewOptions
    {
        /// <summary>
        /// X5 内核 APK 下载地址（ARM64）
        /// </summary>
        public string? X5KernelUrlArm64 { get; set; } = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm64_046295.tbs.apk";

        /// <summary>
        /// X5 内核 APK 下载地址（ARM32）
        /// </summary>
        public string? X5KernelUrlArm { get; set; } = "https://gitee.com/shaonianzhentan/app-store/releases/download/1.0.0/arm_045912_x5.tbs.apk";

        /// <summary>
        /// 下载进度变化回调（0-100）
        /// </summary>
        public Action<int>? OnX5DownloadProgress { get; set; }

        /// <summary>
        /// 下载完成回调（true 表示成功，false 表示失败）
        /// </summary>
        public Action<bool>? OnX5DownloadCompleted { get; set; }

        /// <summary>
        /// 根据当前 CPU 架构获取对应的 X5 APK 下载地址
        /// </summary>
        public string? GetX5KernelUrl()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return X5KernelUrlArm64;
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                return X5KernelUrlArm;
            
            return null;
        }

        /// <summary>
        /// 判断当前 Android 版本是否需要下载 X5 内核
        /// Android 9 (API 28) 以下需要下载
        /// </summary>
        public static bool ShouldDownloadX5Kernel()
        {
#if ANDROID
            return (int)Android.OS.Build.VERSION.SdkInt < 28;
#else
            return false;
#endif
        }
    }
}
