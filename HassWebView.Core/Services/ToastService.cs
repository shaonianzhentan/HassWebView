#if ANDROID
using Android.Widget;
#elif IOS
using UIKit;
#elif WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
#endif

namespace HassWebView.Core.Services
{
    public static class ToastService
    {
        public static void Show(string message)
        {
#if ANDROID
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Platform.AppContext, message, ToastLength.Long).Show();
            });
#elif IOS
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ShowAlert(message, 2.0);
            });
#elif WINDOWS
            var notificationBuilder = new AppNotificationBuilder()
                .AddText(message);
            var notification = notificationBuilder.BuildNotification();
            AppNotificationManager.Default.Show(notification);
#else
            System.Diagnostics.Debug.WriteLine($"Toast Request: {message}");
#endif
        }

#if IOS
        private static void ShowAlert(string message, double seconds)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            var alertDelay = Foundation.NSTimer.CreateScheduledTimer(seconds, (ignore) =>
            {
                alert.DismissViewController(true, null);
            });

            var rootController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
            if (rootController != null)
            {
                var topController = rootController;
                while (topController.PresentedViewController != null)
                {
                    topController = topController.PresentedViewController;
                }
                topController.PresentViewController(alert, true, null);
            }
        }
#endif
    }
}
