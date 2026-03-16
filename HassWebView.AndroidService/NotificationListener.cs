using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Service.Notification;
using HassWebView.AndroidService.NotificationForwarding;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.AndroidService
{
    [Service(Name = "HassWebView.AndroidService.NotificationListener",
             Label = "HassWebView Notification Listener",
             Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class NotificationListener : NotificationListenerService
    {
        private INotificationForwardingService _forwardingService;

        public override void OnCreate()
        {
            base.OnCreate();
            // 从共享的 DI 容器中获取服务实例
            _forwardingService = MauiApplication.Current.Services.GetRequiredService<INotificationForwardingService>();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (sbn?.Notification == null || _forwardingService == null)
            {
                return;
            }

            // 提取核心数据
            var extras = sbn.Notification.Extras;
            var title = extras.GetString(Notification.ExtraTitle);
            var text = extras.GetString(Notification.ExtraText);

            // 如果没有标题或文本，则忽略
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var notificationData = new NotificationData
            {
                PackageName = sbn.PackageName,
                PostTime = sbn.PostTime,
                Title = title,
                Text = text,
                LargeIcon = GetBitmapBytes(extras.GetParcelable(Notification.ExtraLargeIcon) as Bitmap),
                Picture = GetBitmapBytes(extras.GetParcelable(Notification.ExtraPicture) as Bitmap)
            };

            // 使用 Task.Run 在后台线程上触发异步转发，以避免阻塞主线程
            Task.Run(() => _forwardingService.ForwardNotificationAsync(notificationData));
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            // 目前我们不处理通知被移除的事件，但可以在这里添加逻辑
            base.OnNotificationRemoved(sbn);
        }

        private byte[] GetBitmapBytes(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                return stream.ToArray();
            }
        }
    }
}
