namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 事件信息模型。
/// 对应 GET /api/events 响应数组中的单个对象。
/// </summary>
public record EventInfo
{
    /// <summary>
    /// 事件名称 (event)
    /// </summary>
    public required string Event { get; init; }

    /// <summary>
    /// 监听该事件的 Listener 数量 (listener_count)
    /// </summary>
    public required int ListenerCount { get; init; }
}