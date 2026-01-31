namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant API 状态响应模型。
/// 对应 GET /api/ 端点的响应：{"message": "API running."}
/// </summary>
public record ApiStatusResponse
{
    /// <summary>
    /// API 状态信息。
    /// 对应 JSON 中的 "message" 字段。
    /// </summary>
    public required string Message { get; init; }
}