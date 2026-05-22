using HassWebView.Core.Controls;

namespace HassWebView.Core.Services
{
    public static class ToastService
    {
        /// <summary>
        /// 显示 Toast 提示。
        /// 通过当前活跃的 WebViewWithCursor 内嵌 Toast 显示，跨平台统一体验。
        /// </summary>
        public static void Show(string message, int durationMs = 2500)
        {
            var control = FindActiveWebViewWithCursor();
            control?.ShowToast(message, durationMs);
        }

        private static WebViewWithCursor FindActiveWebViewWithCursor()
        {
            var navigation = Shell.Current?.Navigation;
            if (navigation == null) return null;

            // 优先从 Modal 栈查找
            if (navigation.ModalStack.Count > 0)
            {
                if (navigation.ModalStack.Last() is ContentPage page)
                {
                    var control = FindWebViewWithCursorInPage(page);
                    if (control != null) return control;
                }
            }

            // 再从当前页查找
            if (Shell.Current.CurrentPage is ContentPage currentPage)
            {
                return FindWebViewWithCursorInPage(currentPage);
            }

            return null;
        }

        private static WebViewWithCursor FindWebViewWithCursorInPage(ContentPage page)
        {
            return FindInChildren(page.Content);
        }

        private static WebViewWithCursor FindInChildren(Microsoft.Maui.IView view)
        {
            if (view is WebViewWithCursor wvc) return wvc;
            if (view is ILayout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Microsoft.Maui.IView childView)
                    {
                        var found = FindInChildren(childView);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }
    }
}
