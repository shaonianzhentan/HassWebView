namespace HassWebView.HassApi.Models;

/// <summary>
/// Webhook 消息的 'update_location' 类型中嵌套的 'data' 负载。
/// </summary>
public record LocationUpdateRequest(
    // 必填字段
    List<double> Gps,               // 当前位置的经纬度数组 [latitude, longitude]
    int GpsAccuracy,                // GPS 精度，单位：米
    int Battery,                    // 设备剩余电量百分比
    
    // 可选字段
    string? LocationName = null,    // 设备所在的 Home Assistant 区域名称
    int? Speed = null,              // 设备速度，单位：米/秒
    int? Altitude = null,           // 设备海拔高度，单位：米
    int? Course = null,             // 设备行进方向，以度为单位
    int? VerticalAccuracy = null    // 海拔高度值的精度，单位：米
);