using System.Text.Json.Serialization;

namespace HassWebView.HassApi.Models;


/// <summary>
/// Webhook 消息中 'register_sensor' 的顶级 'data' 负载。
/// 包含注册新传感器所需的所有配置信息。
/// </summary>
public record RegisterSensorRequest(
    string Name,        // 必需: 传感器的名称
    object State,       // 必需: 传感器的初始状态
    string Type,        // 必需: 传感器的类型，必须是 "binary_sensor" 或 "sensor"
    [property: JsonPropertyName("unique_id")] 
    string UniqueId     // 必需: 传感器在此 App 安装中的唯一标识符
)
{
    /// <summary>
    /// 可选：附加到传感器的属性。
    /// </summary>
    public Dictionary<string, object>? Attributes { get; init; }
    
    /// <summary>
    /// 可选：传感器的设备类别。
    /// </summary>
    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }

    /// <summary>
    /// 可选：Material Design Icon 图标 (必须以 mdi: 开头)。
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 可选：传感器的度量单位。
    /// </summary>
    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; init; }

    /// <summary>
    /// 可选：实体的状态类别 (仅限传感器)。
    /// </summary>
    [JsonPropertyName("state_class")]
    public string? StateClass { get; init; }
    
    /// <summary>
    /// 可选：实体的实体类别 (例如 diagnostic)。
    /// </summary>
    [JsonPropertyName("entity_category")]
    public string? EntityCategory { get; init; }

    /// <summary>
    /// 可选：如果设置为 true，则实体将被禁用。
    /// </summary>
    public bool? Disabled { get; init; }
}