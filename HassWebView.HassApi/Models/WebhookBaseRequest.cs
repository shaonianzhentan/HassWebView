namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 请求的基础结构，包含消息类型。
/// </summary>
public record WebhookBaseRequest(string Type);