namespace HassWebView.HassApi.Models;


/// <summary>
/// Webhook 消息的 'fire_event' 类型中嵌套的 'data' 负载。
/// 包含要触发的事件类型和事件数据。
/// </summary>
public record FireEventRequest(
    string EventType // 要触发的事件类型 (如 my_custom_event)
)
{
    /// <summary>
    /// 可选：发送给事件的数据负载。
    /// 对应 JSON 中的 "event_data"。使用字典存储动态数据。
    /// </summary>
    public Dictionary<string, object>? EventData { get; init; }
}