namespace HassWebView.HassApi.Models;

/// <summary>
/// 对应 HassConfig 中的 "unit_system" 字段，定义了使用的单位系统。
/// </summary>
public record UnitSystem
{
    // 长度单位 (length)
    public required string Length { get; init; }
    
    // 质量单位 (mass)
    public required string Mass { get; init; }
    
    // 温度单位 (temperature)
    public required string Temperature { get; init; }
    
    // 体积单位 (volume)
    public required string Volume { get; init; }
}