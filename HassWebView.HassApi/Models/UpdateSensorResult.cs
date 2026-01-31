namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息 'update_sensor_states' 响应中单个传感器更新的结果。
/// </summary>
public record UpdateSensorResult(
    bool Success // 标记更新是否成功
)
{
    /// <summary>
    /// 可选：如果成功且实体在 Home Assistant 中被禁用，则为 true。
    /// </summary>
    public bool? IsDisabled { get; init; }

    /// <summary>
    /// 可选：如果 Success 为 false，包含错误详情。
    /// </summary>
    public UpdateSensorError? Error { get; init; }
}

/// <summary>
/// 传感器更新失败时的错误详情。
/// </summary>
public record UpdateSensorError(
    string Code,    // 错误代码 (如 not_registered)
    string Message  // 错误消息
);