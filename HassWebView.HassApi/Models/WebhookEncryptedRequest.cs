namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 加密请求模型。
/// 只有当注册时收到 secret 密钥，且客户端支持加密时才应使用。
/// </summary>
public record WebhookEncryptedRequest(string EncryptedData) 
    : WebhookBaseRequest("encrypted")
{
    /// <summary>
    /// 标记此消息是否已加密，值始终为 true。
    /// </summary>
    public bool Encrypted { get; } = true;
}