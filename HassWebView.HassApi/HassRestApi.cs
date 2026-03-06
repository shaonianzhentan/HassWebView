using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web; // 引入 System.Web 用于 URL 编码
using HassWebView.HassApi.Models;

namespace HassWebView.HassApi;

/// <summary>
/// Home Assistant REST API 客户端
/// </summary>
public class HassRestApi: HttpClientBase
{
    /// <summary>
    /// 初始化 HassRestApi
    /// </summary>
    /// <param name="baseUrl">HA 地址 (例如: http://192.168.1.5:8123)</param>
    /// <param name="tokenRefreshCallback">一个用于在需要时自动获取新令牌的回调函数。bool 参数表示是否需要强制刷新。</param>
    public HassRestApi(string baseUrl, Func<bool, Task<string>>? tokenRefreshCallback = null)
        : base(baseUrl, tokenRefreshCallback)
    {
        // The constructor now simply passes the required parameters to the base class.
    }

    // --- 核心状态 API ---

    /// <summary>
    /// 检查 API 是否连通
    /// GET /api/
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>API 状态响应模型。</returns>
    public async Task<ApiStatusResponse?> GetApiStatusAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<ApiStatusResponse>("api/", cancellationToken);
    }

    // 在 HassRestApi.cs 中添加以下方法：

    /// <summary>
    /// 注册移动应用设备。此接口用于获取后续通信所需的 Webhook ID 和 URL。
    /// POST /api/mobile_app/registrations
    /// </summary>
    /// <param name="request">设备注册请求体。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>注册成功的响应，包含 webhook_id 和 URLs。</returns>
    public async Task<MobileAppRegistrationResponse?> RegisterMobileAppAsync(
        MobileAppRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        // This method requires an authenticated request, the base class will handle token acquisition.
        return await PostJsonAsync<MobileAppRegistrationRequest, MobileAppRegistrationResponse>(
            "api/mobile_app/registrations", 
            request, 
            cancellationToken);
    }

    /// <summary>
    /// 获取所有设备的状态
    /// GET /api/states
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>所有实体状态的列表。</returns>
    public async Task<List<HassState>> GetAllStatesAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync<List<HassState>>("api/states", cancellationToken);
        return result ?? new List<HassState>();
    }

    /// <summary>
    /// 获取指定设备的状态
    /// GET /api/states/{entity_id}
    /// </summary>
    /// <param name="entityId">要查询的实体 ID。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>单个实体状态。</returns>
    public async Task<HassState?> GetStateAsync(string entityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        return await GetJsonAsync<HassState>($"api/states/{entityId}", cancellationToken);
    }

    // --- 配置/服务/事件 API ---

    /// <summary>
    /// 获取 Home Assistant 的当前配置信息。
    /// GET /api/config
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>配置信息模型。</returns>
    public async Task<HassConfig?> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<HassConfig>("api/config", cancellationToken);
    }

    /// <summary>
    /// 获取所有可用的事件信息。
    /// GET /api/events
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>事件信息列表。</returns>
    public async Task<List<EventInfo>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync<List<EventInfo>>("api/events", cancellationToken);
        return result ?? new List<EventInfo>();
    }

    /// <summary>
    /// 获取所有可用的服务领域及其包含的服务。
    /// GET /api/services
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>服务领域信息列表。</returns>
    public async Task<List<ServiceDomainInfo>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync<List<ServiceDomainInfo>>("api/services", cancellationToken);
        return result ?? new List<ServiceDomainInfo>();
    }

    /// <summary>
    /// 调用服务 (执行动作)
    /// POST /api/services/{domain}/{service}
    /// </summary>
    /// <param name="domain">领域 (如 light, switch)</param>
    /// <param name="service">服务名 (如 turn_on, toggle)</param>
    /// <param name="payload">参数对象 (可以是匿名对象)</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>受影响的实体最新状态列表。</returns>
    public async Task<List<HassState>> CallServiceAsync(string domain, string service, object? payload = null, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/services/{domain}/{service}";

        var result = await PostJsonAsync<object, List<HassState>>(endpoint, payload, cancellationToken);
        return result ?? new List<HassState>();
    }

    // --- 历史记录/日志 API ---

    /// <summary>
    /// 获取一段时间内的实体状态历史记录。
    /// GET /api/history/period/{timestamp}
    /// </summary>
    /// <param name="start">历史记录查询的开始时间。可选，默认 1 天前。</param>
    /// <param name="entityIds">要查询的一个或多个实体ID，以逗号分隔。</param>
    /// <param name="end">历史记录查询的结束时间。可选，默认 1 天后。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>双层列表，包含每个时间点的状态变化。</returns>
    public async Task<List<List<HassState>>?> GetHistoryAsync(
        DateTimeOffset? start = null,
        string? entityIds = null,
        DateTimeOffset? end = null,
        bool minimalResponse = false,
        bool noAttributes = false,
        bool significantChangesOnly = false,
        CancellationToken cancellationToken = default)
    {
        var endpoint = new StringBuilder("api/history/period/");
        if (start.HasValue)
        {
            // URL 编码时间戳，并追加到路径中
            endpoint.Append(HttpUtility.UrlEncode(start.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")));
        }

        var query = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrWhiteSpace(entityIds))
            query["filter_entity_id"] = entityIds;

        if (end.HasValue)
            query["end_time"] = HttpUtility.UrlEncode(end.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"));

        if (minimalResponse)
            query["minimal_response"] = "1";

        if (noAttributes)
            query["no_attributes"] = "1";

        if (significantChangesOnly)
            query["significant_changes_only"] = "1";

        if (query.Count > 0)
        {
            // System.Web.HttpUtility.UrlEncode 在 netstandard2.0/net8.0 中可用，但需要引用 System.Web 包
            endpoint.Append($"?{query}");
        }

        return await GetJsonAsync<List<List<HassState>>>(endpoint.ToString(), cancellationToken);
    }

    /// <summary>
    /// 获取一段时间内的日志记录条目。
    /// GET /api/logbook/{timestamp}
    /// </summary>
    /// <param name="start">日志查询的开始时间。可选，默认 1 天前。</param>
    /// <param name="entityId">可选，要过滤的单个实体ID。</param>
    /// <param name="end">可选，日志查询的结束时间。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>日志记录条目列表。</returns>
    public async Task<List<LogbookEntry>> GetLogbookAsync(
        DateTimeOffset? start = null,
        string? entityId = null,
        DateTimeOffset? end = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = new StringBuilder("api/logbook/");
        if (start.HasValue)
        {
            endpoint.Append(HttpUtility.UrlEncode(start.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")));
        }

        var query = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrWhiteSpace(entityId))
            query["entity"] = entityId;

        if (end.HasValue)
            query["end_time"] = HttpUtility.UrlEncode(end.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"));

        if (query.Count > 0)
        {
            endpoint.Append($"?{query}");
        }

        var result = await GetJsonAsync<List<LogbookEntry>>(endpoint.ToString(), cancellationToken);
        return result ?? new List<LogbookEntry>();
    }

    // --- 日历 API ---

    /// <summary>
    /// 获取所有可用的日历实体信息。
    /// GET /api/calendars
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>日历实体信息列表。</returns>
    public async Task<List<CalendarInfo>> GetCalendarsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync<List<CalendarInfo>>("api/calendars", cancellationToken);
        return result ?? new List<CalendarInfo>();
    }

    /// <summary>
    /// 获取指定日历实体在一段时间内的事件列表。
    /// GET /api/calendars/{entity_id}
    /// </summary>
    /// <param name="entityId">日历实体ID (例如 calendar.holidays)。</param>
    /// <param name="start">查询的开始时间 (必填)。</param>
    /// <param name="end">查询的结束时间 (必填)。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>日历事件列表。</returns>
    public async Task<List<CalendarEvent>> GetCalendarEventsAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        var endpoint = $"api/calendars/{entityId}";

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["start"] = HttpUtility.UrlEncode(start.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        query["end"] = HttpUtility.UrlEncode(end.ToString("yyyy-MM-ddTHH:mm:sszzz"));

        endpoint = $"{endpoint}?{query}";

        var result = await GetJsonAsync<List<CalendarEvent>>(endpoint, cancellationToken);
        return result ?? new List<CalendarEvent>();
    }

    // --- 模板/配置检查 API ---

    /// <summary>
    /// 渲染一个 Home Assistant 模板。
    /// </summary>
    /// <param name="request">包含要渲染的模板字符串的请求体。</param>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>渲染后的模板字符串。</returns>
    public async Task<string?> RenderTemplateAsync(TemplateRenderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequestAsync(() => RawClient.PostAsJsonAsync("api/template", request, SnakeCaseJsonOptions, cancellationToken));
        if(response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return null;
    }

    /// <summary>
    /// 触发配置文件的核心检查。
    /// </summary>
    /// <param name="cancellationToken">用于取消长时间运行的操作的令牌。</param>
    /// <returns>包含检查结果和错误信息的响应模型。</returns>
    public async Task<ConfigCheckResponse?> CheckConfigAsync(CancellationToken cancellationToken = default)
    {
        return await PostJsonAsync<object, ConfigCheckResponse>("api/config/core/check_config", null, cancellationToken);
    }
}
