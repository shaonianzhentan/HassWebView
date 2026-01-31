namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息 'update_sensor_states' 数组中的单个传感器更新负载。
/// 用于更新已注册传感器的当前状态。
/// </summary>
public record UpdateSensorRequest(
    object State,       // 必需: 传感器的当前状态
    string Type,        // 必需: 传感器的类型，必须是 "binary_sensor" 或 "sensor"
    string UniqueId     // 必需: 传感器在此 App 安装中的唯一标识符
)
{
    /// <summary>
    /// 可选：附加到传感器的属性。
    /// </summary>
    public Dictionary<string, object>? Attributes { get; init; }

    /// <summary>
    /// 可选：Material Design Icon 图标 (必须以 mdi: 开头)。
    /// </summary>
    public string? Icon { get; init; }
}