namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息中用于更新推送令牌和 URL 的负载结构。
/// 对应 JSON 中 "app_data"。
/// </summary>
/// <param name="PushToken">必填：推送通知令牌。</param>
/// <param name="PushUrl">必填：推送通知 URL。</param>
public record MobileAppData(
    string PushToken,
    string PushUrl
);