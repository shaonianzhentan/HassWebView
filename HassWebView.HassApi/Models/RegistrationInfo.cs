namespace HassWebView.HassApi.Models;

/// <summary>
/// 移动应用注册信息模型，嵌套在 NotificationPayload 中。
/// </summary>
public record RegistrationInfo(
    string AppId,
    string AppVersion,
    string OsVersion,
    string WebhookId 
);