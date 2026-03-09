using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System;
using HassWebView.HassApi.Models;
using System.Net;
using System.Diagnostics;

namespace HassWebView.HassApi;

/// <summary>
/// Home Assistant OAuth2 授权服务客户端。
/// </summary>
public class HassAuth: HttpClientBase 
{
    /// <summary>
    /// 编码后的 client_id
    /// </summary>
    readonly string clientId;

    /// <summary>
    /// 重定向URI，获取code码（示例：http://homeassistant.local:8123/?external_auth=1）
    /// </summary>
    public readonly string RedirectUri;
    /// <summary>
    /// 授权URI（示例：http://homeassistant.local:8123/auth/authorize?client_id={clientId}&redirect_uri={RedirectUri}）
    /// </summary>
    public readonly string AuthorizeUri;

    /// <summary>
    /// 初始化 HassAuth。
    /// </summary>
    public HassAuth(string baseUrl): base(baseUrl)
    {
        // 使用继承的 BaseUrl 属性进行 URL 拼接
        clientId = Uri.EscapeDataString(this.BaseUrl);
        RedirectUri = $"{this.BaseUrl}?external_auth=1"; 
        AuthorizeUri = $"{this.BaseUrl}auth/authorize?response_type=code&client_id={clientId}&redirect_uri={Uri.EscapeDataString(RedirectUri)}";
    }

    /// <summary>
    /// 检查 Home Assistant API 是否可访问且有效。
    /// </summary>
    /// <returns>如果 API 端点返回预期的状态码(401 Unauthorized)，则返回 true；否则返回 false。</returns>
    public async Task<bool> CheckApiStatusAsync()
    {
        try
        {
            var response = await RawClient.GetAsync("api/");
            return response.StatusCode == HttpStatusCode.Unauthorized;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HassAuth] API status check failed for {BaseUrl}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 内部通用方法，用于向 /auth/token 端点发送 POST 请求 (Content-Type: application/x-www-form-urlencoded)。
    /// </summary>
    private async Task<AuthorizationResult?> PostAuthTokenAsync(string queryString)
    {
        // 创建 Content，指定 Content-Type 为 form-urlencoded
        using var httpContent = new StringContent(queryString, Encoding.UTF8, "application/x-www-form-urlencoded");
        
        // 直接使用相对路径字符串，利用 RawClient 的 BaseAddress
        HttpResponseMessage response = await RawClient.PostAsync("auth/token", httpContent);

        response.EnsureSuccessStatusCode(); 

        string responseContent = await response.Content.ReadAsStringAsync();
        
        // 反序列化：将 string 转换为 Stream 以兼容 HassJsonHelper.DeserializeAsync<T>(Stream)
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
        
        return await HassJsonHelper.DeserializeAsync<AuthorizationResult>(stream);
    }
    
    // --- 公共授权接口 (保持不变) ---

    /// <summary>
    /// 使用授权码 (Code) 交换访问令牌 (Access Token) 和刷新令牌 (Refresh Token)。
    /// </summary>
    public Task<AuthorizationResult?> GetRefreshTokenAsync(string code)
    {
        string queryString = $"grant_type=authorization_code&code={code}&client_id={clientId}";
        return PostAuthTokenAsync(queryString);
    }

    /// <summary>
    /// 使用刷新令牌 (Refresh Token) 获取新的访问令牌 (Access Token)。
    /// </summary>
    public Task<AuthorizationResult?> GetAccessTokenAsync(string refreshToken)
    {
        string queryString = $"grant_type=refresh_token&refresh_token={refreshToken}&client_id={clientId}";
        return PostAuthTokenAsync(queryString);
    }

    /// <summary>
    /// 撤销指定的刷新令牌 (Refresh Token)。
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        string queryString = $"token={refreshToken}&action=revoke";
        await PostAuthTokenAsync(queryString); 
    }
}