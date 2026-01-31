namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息的 'call_service' 类型中嵌套的 'data' 负载。
/// 包含要调用的服务信息。
/// </summary>
public record CallServiceRequest(
    string Domain, // 服务的领域 (如 light, switch)
    string Service // 服务动作名称 (如 turn_on, toggle)
)
{
    /// <summary>
    /// 可选：发送给服务动作的数据负载 (如 entity_id, brightness, etc.)。
    /// 对应 JSON 中的 "service_data"。使用字典存储动态数据。
    /// </summary>
    public Dictionary<string, object>? ServiceData { get; init; }
}