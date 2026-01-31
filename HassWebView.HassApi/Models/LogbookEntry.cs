using System;
using System.Collections.Generic;

namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 日志记录条目模型 (Logbook Entry)。
/// 对应 GET /api/logbook/<timestamp> 响应数组中的单个对象。
/// </summary>
public record LogbookEntry
{
    // 触发此日志条目的用户ID，可能为 null
    public string? ContextUserId { get; init; }

    // 实体所属的领域 (例如 light, sensor, alarm_control_panel)
    public required string Domain { get; init; }

    // 发生变化的实体ID
    public required string EntityId { get; init; }

    // 日志消息文本 (例如 "changed to disarmed")
    public required string Message { get; init; }

    // 日志记录的名称/来源 (例如 "Security", "HomeKit")
    public required string Name { get; init; }

    // 发生时间 (when)
    public required DateTimeOffset When { get; init; }
}