using System.Collections.Generic;
namespace HassWebView.HassApi.Models;


/// <summary>
/// Home Assistant 服务信息模型。
/// 对应 GET /api/services 响应数组中的单个对象。
/// </summary>
public record ServiceDomainInfo
{
    /// <summary>
    /// 服务所属的领域 (例如 light, switch, browser)。
    /// 对应 JSON 中的 "domain"。
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// 该领域下可用的服务名称列表 (例如 turn_on, volume_up)。
    /// 对应 JSON 中的 "services"。
    /// </summary>
    public List<string> Services { get; init; } = new();
}