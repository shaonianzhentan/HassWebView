namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 通用请求模型 (非加密)。
/// </summary>
/// <typeparam name="TData">data 字段的具体负载类型。</typeparam>
public record WebhookRequest<TData>(string Type, TData Data) : WebhookBaseRequest(Type);