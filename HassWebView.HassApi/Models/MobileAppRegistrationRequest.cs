namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 移动应用设备注册请求体模型。
/// 对应 POST /api/mobile_app/registrations 接口的请求 JSON Payload。
/// 所有字段为必填项（除非有默认值），依赖全局 SnakeCase 序列化策略。
/// </summary>
public class MobileAppRegistrationRequest
{
    /// <summary>
    /// 必填：设备的唯一标识符（对应 JSON 中的 "device_id"）。
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// 必填：移动应用的唯一标识符（对应 JSON 中的 "app_id"）。
    /// </summary>
    public required string AppId { get; init; }

    /// <summary>
    /// 必填：移动应用的名称（对应 JSON 中的 "app_name"）。
    /// </summary>
    public required string AppName { get; init; }

    /// <summary>
    /// 必填：移动应用的当前版本（对应 JSON 中的 "app_version"）。
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
    /// 必填：操作系统名称（对应 JSON 中的 "os_name"）。
    /// </summary>
    public required string OsName { get; init; }

    /// <summary>
    /// 必填：操作系统版本（对应 JSON 中的 "os_version"）。
    /// </summary>
    public required string OsVersion { get; init; }

    /// <summary>
    /// 必填：应用是否支持加密（对应 JSON 中的 "supports_encryption"）。
    /// </summary>
    public required bool SupportsEncryption { get; init; }

    /// <summary>
    /// 必填：应用扩展数据 (如推送通知密钥)（对应 JSON 中的 "app_data"）。
    /// </summary>
    public required MobileAppData AppData { get; init; }
}