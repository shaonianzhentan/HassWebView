namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 配置检查响应模型。
/// 对应 POST /api/config/core/check_config 接口的响应。
/// </summary>
public record ConfigCheckResponse
{
    /// <summary>
    /// 检查结果，通常为 "valid" 或 "invalid"。
    /// 对应 JSON 中的 "result"。
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// 配置检查失败时的错误信息字符串。
    /// 检查成功时该字段为 null。
    /// 对应 JSON 中的 "errors"。
    /// </summary>
    public string? Errors { get; init; }
}