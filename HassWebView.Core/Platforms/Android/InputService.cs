using Android.Views.InputMethods;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;

namespace HassWebView.Core
{
    public static partial class InputService
    {
        static partial void PlatformInjectText(string text)
        {
            var inputMethodManager = MauiApplication.Context.GetSystemService(Android.Content.Context.InputMethodService) as InputMethodManager;
            var view = Platform.CurrentActivity.CurrentFocus;
            if (view != null)
            {
                inputMethodManager.ShowSoftInput(view, ShowFlags.Implicit);
                var inputConnection = view.OnCreateInputConnection(new EditorInfo());
                inputConnection?.CommitText(text, 1);
            }
        }
    }
}
