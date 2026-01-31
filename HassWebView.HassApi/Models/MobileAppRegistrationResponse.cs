namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 移动应用设备注册成功的响应体模型。
/// 对应 POST /api/mobile_app/registrations 接口的成功响应。
/// </summary>
/// <param name="WebhookId">必填：注册后分配给设备的 Webhook ID，用于后续的 API 交互。映射到 JSON "webhook_id"。</param>
/// <param name="Secret">必填：Webook 加密密钥，用于加密或验证 Webhook 签名。</param>
/// <param name="CloudhookUrl">如果使用了 Home Assistant Cloud，则为云 Webhook URL。映射到 JSON "cloudhook_url"。</param>
/// <param name="RemoteUiUrl">远程 UI 访问 URL。映射到 JSON "remote_ui_url"。</param>
public record MobileAppRegistrationResponse(
    string WebhookId,
    string Secret,
    string CloudhookUrl,
    string RemoteUiUrl
);