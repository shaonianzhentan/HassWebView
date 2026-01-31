namespace HassWebView.HassApi.Models;

/// <summary>
/// 日历事件模型。
/// 对应 GET /api/calendars/<entity_id> 响应数组中的单个事件。
/// </summary>
public record CalendarEvent
{
    /// <summary>
    /// 事件摘要或标题 (summary)。
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// 事件的开始时间对象。
    /// </summary>
    public required CalendarEventTime Start { get; init; } = new();

    /// <summary>
    /// 事件的结束时间对象。
    /// </summary>
    public required CalendarEventTime End { get; init; } = new();

    /// <summary>
    /// 事件的描述信息 (description)。可能为 null。
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 事件的地点信息 (location)。可能为 null。
    /// </summary>
    public string? Location { get; init; }
    
}