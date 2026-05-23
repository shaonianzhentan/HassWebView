using HassWebView.Core.Controls;

namespace HassWebView.Core.Services
{
    public static class ToastService
    {
        /// <summary>
        /// 显示 Toast 提示。
        /// 通过传入的 WebViewWithCursor 内嵌 Toast 显示，跨平台统一体验。
        /// </summary>
        public static void Show(WebViewWithCursor control, string message, int durationMs = 2500)
        {
            control?.ShowToast(message, durationMs);
        }
    }
}
