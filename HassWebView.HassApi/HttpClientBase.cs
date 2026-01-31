using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HassWebView.HassApi;

/// <summary>
/// 所有 Home Assistant 客户端模块（HassClient, MobileAppClient, HassAuth）的基类。
/// 在内部实例化并配置一个 HttpClient 实例，并提供核心 HTTP 封装方法。
/// </summary>
public abstract class HttpClientBase
{
    // 保护字段：持有 HttpClient 实例
    protected readonly HttpClient RawClient;

    /// <summary>
    /// HA 地址 (例如: http://192.168.1.5:8123)
    /// </summary>
    protected readonly string baseUrl;
    
    /// <summary>
    /// 初始化 HttpClientBase，并在内部创建和配置新的 HttpClient 实例。
    /// </summary>
    /// <param name="baseUrl">HA 地址 (例如: http://192.168.1.5:8123)</param>
    public HttpClientBase(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));
        
        RawClient = new HttpClient(); 
        this.baseUrl = baseUrl.TrimEnd('/');
        RawClient.BaseAddress = new Uri(this.baseUrl + "/");
        
        RawClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    /// <summary>
    /// 设置或移除默认的 Bearer 授权令牌。
    /// </summary>
    /// <param name="accessToken">要设置的令牌，如果为 null 或空则移除授权头。</param>
    public void SetAuthorizationToken(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            RawClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            RawClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
    
    // ----------------------------------------------------------------
    // 核心 HTTP 方法（Protected 封装）
    // ----------------------------------------------------------------
    
    protected async Task<T?> GetJsonAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class
    {
        var response = await RawClient.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error GET {endpoint}: {response.StatusCode} - {errorMsg}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        return await HassJsonHelper.DeserializeAsync<T>(stream, cancellationToken);
    }

    protected async Task<T?> PostJsonAsync<T>(string endpoint, object? payload, CancellationToken cancellationToken = default) where T : class
    {
        var jsonContent = payload is not null
            ? HassJsonHelper.Serialize(payload)
            : "{}"; 

        using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await RawClient.PostAsync(endpoint, httpContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error POST {endpoint}: {response.StatusCode} - {errorMsg}");
        }

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        using var responseStream = await response.Content.ReadAsStreamAsync();
        return await HassJsonHelper.DeserializeAsync<T>(responseStream, cancellationToken);
    }

    protected async Task<string> PostRawAsync(string endpoint, object? payload, CancellationToken cancellationToken = default)
    {
        var jsonContent = payload is not null
            ? HassJsonHelper.Serialize(payload)
            : "{}";

        using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await RawClient.PostAsync(endpoint, httpContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error POST {endpoint}: {response.StatusCode} - {errorMsg}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}