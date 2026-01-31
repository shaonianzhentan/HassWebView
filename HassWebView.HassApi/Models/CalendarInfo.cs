namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 日历实体信息模型。
/// 对应 GET /api/calendars 响应数组中的单个对象。
/// </summary>
public record CalendarInfo
{
    /// <summary>
    /// 日历实体的唯一标识符 (entity_id)，例如 calendar.holidays。
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// 日历实体的友好名称 (name)，例如 National Holidays。
    /// </summary>
    public required string Name { get; init; }
}