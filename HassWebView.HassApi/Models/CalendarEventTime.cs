namespace HassWebView.HassApi.Models;

/// <summary>
/// 日历事件的开始或结束时间模型。
/// 对应 JSON 中的 "start" 或 "end" 字段。
/// </summary>
public record CalendarEventTime
{
    /// <summary>
    /// 日期时间戳，用于有时长/时区的事件 (例如 "2022-05-06T20:00:00-07:00")。
    /// 仅在非全天事件中出现。
    /// </summary>
    public DateTimeOffset? DateTime { get; init; }

    /// <summary>
    /// 仅日期字符串 (例如 "2022-05-05")，用于全天事件。
    /// 仅在全天事件中出现。
    /// </summary>
    // 关键修复：使用 string? 兼容旧框架
    public string? Date { get; init; } 
}