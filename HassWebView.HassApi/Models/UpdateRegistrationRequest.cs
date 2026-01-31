namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息的 'update_registration' 类型中嵌套的 'data' 负载。
/// 用于更新设备信息或 App 数据，所有字段为必填项，依赖全局 SnakeCase 序列化策略（属性名自动映射为下划线命名格式）。
/// </summary>
public class UpdateRegistrationRequest
{
    /// <summary>
    /// 必填：用于更新推送信息的 App 数据（对应 JSON 中的 "app_data"）。
    /// </summary>
    public required MobileAppData AppData { get; init; }

    /// <summary>
    /// 必填：移动 App 的版本（对应 JSON 中的 "app_version"）。
    /// </summary>
    public required string AppVersion { get; init; }

    /// <summary>
    /// 必填：设备的名称（对应 JSON 中的 "device_name"）。
    /// </summary>
    public required string DeviceName { get; init; }

    /// <summary>
    /// 必填：设备制造商（对应 JSON 中的 "manufacturer"）。
    /// </summary>
    public required string Manufacturer { get; init; }

    /// <summary>
    /// 必填：设备型号（对应 JSON 中的 "model"）。
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// 必填：操作系统版本（对应 JSON 中的 "os_version"）。
    /// </summary>
    public required string OsVersion { get; init; }
}