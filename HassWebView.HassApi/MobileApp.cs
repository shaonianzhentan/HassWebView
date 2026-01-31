using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HassWebView.HassApi.Models;

namespace HassWebView.HassApi;

/// <summary>
/// Home Assistant 移动应用 Webhook 交互处理类。
/// 负责发送未认证的 Webhook 消息，使用持久化存储的配置进行实例化。
/// </summary>
public class MobileApp: HttpClientBase
{
    private readonly string _webhookId;

    /// <summary>
    /// 私有属性：用于构造 Webhook 完整 URL 的相对路径。
    /// </summary>
    private string WebhookUrl => $"/api/webhook/{_webhookId}";

    /// <summary>
    /// 初始化 MobileApp，用于后续的 Webhook 交互。
    /// </summary>
    /// <param name="baseUrl">Home Assistant 的基础 URL。</param>
    /// <param name="webhookId">设备注册成功后持久化存储的 Webhook ID。</param>
    public MobileApp(string baseUrl, string webhookId): base(baseUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookId)) 
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(webhookId));

        _webhookId = webhookId;
        // Webhook 是未认证的，确保移除基类可能设置的 Bearer 认证头。
        SetAuthorizationToken(null);
    }

    // --- 低层级通用发送方法 (统一后的核心方法) ---

    /// <summary>
    /// Webhook POST 请求的核心执行方法。发送 JSON Payload，并根据 expectResponse 处理返回（未认证）。
    /// </summary>
    /// <typeparam name="TResponse">期望的 JSON 响应类型。</typeparam>
    private async Task<TResponse?> PostWebhookCoreAsync<TResponse>(
        object payload,
        CancellationToken cancellationToken,
        bool expectResponse = true)
        where TResponse : class
    {
        var jsonContent = HassJsonHelper.Serialize(payload);
        using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            // 使用继承的 RawClient 替换旧的 _httpClient
            response = await RawClient.PostAsync(WebhookUrl, httpContent, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Webhook Error POST {WebhookUrl}: Connection failed or unexpected error.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Webhook Error POST {WebhookUrl}: {response.StatusCode} - {errorMsg}");
        }

        if (!expectResponse)
        {
            return null;
        }

        // 只有 expectResponse 为 true 且内容长度非零时才尝试反序列化
        if (response.Content.Headers.ContentLength == 0)
        {
            return null;
        }

        using var responseStream = await response.Content.ReadAsStreamAsync();
        return await HassJsonHelper.DeserializeAsync<TResponse>(responseStream, cancellationToken);
    }

    // --- 高层级 API (发送/void) ---
    // 这些方法现在直接调用 PostWebhookCoreAsync<object>(..., expectResponse: false)

    /// <summary>
    /// 发送设备的位置更新信息。
    /// </summary>
    public Task SendLocationUpdateAsync(
        LocationUpdateRequest locationData,
        CancellationToken cancellationToken = default)
    {
        if (locationData is null) throw new ArgumentNullException(nameof(locationData));
        var locationPayload = new WebhookRequest<LocationUpdateRequest>("update_location", locationData);
        // 调用核心方法，不期望响应
        return PostWebhookCoreAsync<object>(locationPayload, cancellationToken, expectResponse: false);
    }

    /// <summary>
    /// 通过 Webhook 调用 Home Assistant 中的一个服务操作。
    /// </summary>
    public Task CallServiceActionAsync(
        CallServiceRequest serviceData,
        CancellationToken cancellationToken = default)
    {
        if (serviceData is null) throw new ArgumentNullException(nameof(serviceData));
        var servicePayload = new WebhookRequest<CallServiceRequest>("call_service", serviceData);
        // 调用核心方法，不期望响应
        return PostWebhookCoreAsync<object>(servicePayload, cancellationToken, expectResponse: false);
    }

    /// <summary>
    /// 通过 Webhook 触发 Home Assistant 中的一个事件。
    /// </summary>
    public Task FireEventAsync(
        FireEventRequest eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData is null) throw new ArgumentNullException(nameof(eventData));
        var eventPayload = new WebhookRequest<FireEventRequest>("fire_event", eventData);
        // 调用核心方法，不期望响应
        return PostWebhookCoreAsync<object>(eventPayload, cancellationToken, expectResponse: false);
    }

    /// <summary>
    /// 通过 Webhook 更新已注册设备的信息 (如 App 版本、设备名称或推送令牌)。
    /// </summary>
    public Task UpdateRegistrationAsync(
        UpdateRegistrationRequest updateData,
        CancellationToken cancellationToken = default)
    {
        if (updateData is null) throw new ArgumentNullException(nameof(updateData));
        var updatePayload = new WebhookRequest<UpdateRegistrationRequest>("update_registration", updateData);
        // 调用核心方法，不期望响应
        return PostWebhookCoreAsync<object>(updatePayload, cancellationToken, expectResponse: false);
    }
    
    /// <summary>
    /// 通过 Webhook 注册一个新的传感器或二进制传感器。
    /// </summary>
    public Task RegisterSensorAsync(
        RegisterSensorRequest sensorData,
        CancellationToken cancellationToken = default)
    {
        if (sensorData is null) throw new ArgumentNullException(nameof(sensorData));
        var registerPayload = new WebhookRequest<RegisterSensorRequest>("register_sensor", sensorData);
        // 调用核心方法，不期望响应
        return PostWebhookCoreAsync<object>(registerPayload, cancellationToken, expectResponse: false);
    }

    // --- 高层级 API (获取/带响应) ---
    // 这些方法现在直接调用 PostWebhookCoreAsync<TResponse>(...)

    /// <summary>
    /// 通过 Webhook 渲染一个或多个 Home Assistant 模板。
    /// </summary>
    /// <returns>返回包含渲染结果的字典。</returns>
    public Task<Dictionary<string, string>?> RenderTemplatesAsync(
        Dictionary<string, TemplateData> templatesData,
        CancellationToken cancellationToken = default)
    {
        if (templatesData is null || templatesData.Count == 0)
        {
            throw new ArgumentException("Templates data cannot be null or empty.", nameof(templatesData));
        }

        var renderData = new RenderTemplateRequest(templatesData);
        var renderPayload = new WebhookRequest<RenderTemplateRequest>("render_template", renderData);

        // 调用核心方法，期望响应
        return PostWebhookCoreAsync<Dictionary<string, string>>(renderPayload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 获取所有启用的区域（Zones）。
    /// </summary>
    /// <returns>返回 Zone 实体列表。</returns>
    public Task<List<object>?> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        var request = new WebhookBaseRequest("get_zones");
        // 调用核心方法，期望响应
        return PostWebhookCoreAsync<List<object>>(request, cancellationToken);
    }


    /// <summary>
    /// 通过 Webhook 获取 Home Assistant 的配置信息。
    /// </summary>
    /// <returns>返回配置实体。</returns>
    public Task<object?> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        var request = new WebhookBaseRequest("get_config");
        // 调用核心方法，期望响应
        return PostWebhookCoreAsync<object>(request, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 批量更新一个或多个已注册传感器的状态和属性。
    /// </summary>
    /// <returns>返回一个字典，其中键是 unique_id，值是更新结果。</returns>
    public Task<Dictionary<string, UpdateSensorResult>?> UpdateSensorsAsync(
        List<UpdateSensorRequest> updates,
        CancellationToken cancellationToken = default)
    {
        if (updates is null || updates.Count == 0)
        {
            throw new ArgumentException("Updates list cannot be null or empty.", nameof(updates));
        }

        var updatePayload = new WebhookRequest<List<UpdateSensorRequest>>("update_sensor_states", updates);

        // 调用核心方法，期望响应
        return PostWebhookCoreAsync<Dictionary<string, UpdateSensorResult>>(updatePayload, cancellationToken);
    }
}