namespace HassWebView.Core.Services
{
    public static class InputService
    {
        public static void InjectText(string text)
        {
#if ANDROID
            var inputMethodManager = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
            var view = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.CurrentFocus;
            if (view != null && inputMethodManager != null)
            {
                inputMethodManager.ShowSoftInput(view, Android.Views.InputMethods.ShowFlags.Implicit);
                var inputConnection = view.OnCreateInputConnection(new Android.Views.InputMethods.EditorInfo());
                inputConnection?.CommitText(text, 1);
            }
#endif
        }
    }
}