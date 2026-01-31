namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant OAuth2 授权接口返回的结果 (简化版，依赖全局 SnakeCase 配置)。
/// </summary>
public record AuthorizationResult(
    // 依赖全局配置将 AccessToken 转换为 access_token
    string AccessToken,
    
    // 依赖全局配置将 ExpiresIn 转换为 expires_in
    int ExpiresIn,
    
    // 依赖全局配置将 TokenType 转换为 token_type
    string TokenType
)
{
    // 依赖全局配置将 RefreshToken 转换为 refresh_token
    public string? RefreshToken { get; init; }
}