namespace HassWebView.HassApi.Models;


/// <summary>
/// HassState: Home Assistant 实体状态模型
/// </summary>
public record HassState
{
    /// <summary>
    /// 实体 ID (对应 JSON 中的 entity_id)
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// 实体状态 (对应 JSON 中的 state)
    /// </summary>
    public string State { get; init; } = "unknown";

    /// <summary>
    /// 最后一次状态变化时间 (对应 JSON 中的 last_changed)
    /// </summary>
    public DateTimeOffset LastChanged { get; init; }

    /// <summary>
    /// 最后一次状态变化时间 (对应 JSON 中的 last_updated)
    /// </summary>
    public DateTimeOffset? LastUpdated { get; init; }
    
    /// <summary>
    /// 属性字典 (对应 JSON 中的 attributes)
    /// </summary>
    public Dictionary<string, object> Attributes { get; init; } = new();
}