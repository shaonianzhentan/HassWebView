namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 配置信息模型 (Config Object)。
/// 对应 GET /api/config 端点的完整响应。
/// </summary>
public record HassConfig
{
    // 已加载的组件列表 (components)
    public List<string> Components { get; init; } = new(); 

    // 配置目录路径 (config_dir)
    public required string ConfigDir { get; init; } 

    // 海拔高度 (elevation)
    public required int Elevation { get; init; } 

    public required double Latitude { get; init; }

    // 位置名称 (location_name)
    public required string LocationName { get; init; }

    public required double Longitude { get; init; }

    // 时区 (time_zone)
    public required string TimeZone { get; init; }

    // 单位系统 (unit_system)，使用嵌套模型
    public required UnitSystem UnitSystem { get; init; }

    // 核心版本号 (version)
    public required string Version { get; init; }

    // 外部目录白名单 (whitelist_external_dirs)
    public List<string> WhitelistExternalDirs { get; init; } = new();
}