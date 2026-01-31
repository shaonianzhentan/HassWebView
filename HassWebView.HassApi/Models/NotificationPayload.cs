namespace HassWebView.HassApi.Models;

/// <summary>
/// 接收 Home Assistant mobile_app.notify 服务发出的推送通知 Payload。
/// </summary>
public record NotificationPayload(
    // 移除属性，使用 PascalCase 命名
    string Message,
    string Title,
    string PushToken,
    RegistrationInfo RegistrationInfo,
    Dictionary<string, object>? Data 
);