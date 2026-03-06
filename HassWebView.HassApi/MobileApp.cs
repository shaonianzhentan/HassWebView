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
/// 负责发送未认证的 Webhook 消息。
/// </summary>
public class MobileApp : HttpClientBase
{
    private readonly string _webhookId;

    /// <summary>
    /// 获取用于构造 Webhook 完整 URL 的相对路径。
    /// </summary>
    private string WebhookUrl => $"/api/webhook/{_webhookId}";

    /// <summary>
    /// 初始化 MobileApp，用于后续的 Webhook 交互。
    /// </summary>
    /// <param name="baseUrl">Home Assistant 的基础 URL。</param>
    /// <param name="webhookId">设备注册成功后持久化存储的 Webhook ID。</param>
    public MobileApp(string baseUrl, string webhookId)
        // Webhook 请求是未经身份验证的，因此我们不向基类提供令牌刷新回调。
        : base(baseUrl, null)
    {
        if (string.IsNullOrWhiteSpace(webhookId)) 
            throw new ArgumentException("Webhook ID cannot be null or whitespace.", nameof(webhookId));

        _webhookId = webhookId;

        // 明确设置 AccessToken 为 null，确保基类不会尝试添加 Authorization 头。
        this.AccessToken = null;
    }

    // --- 核心 Webhook 方法 ---

    /// <summary>
    /// Webhook POST 请求的核心执行方法。
    /// </summary>
    private async Task<TResponse?> PostWebhookAsync<TRequest, TResponse>(TRequest payload, CancellationToken cancellationToken) 
        where TResponse : class
    {
        return await PostJsonAsync<TRequest, TResponse>(WebhookUrl, payload, cancellationToken);
    }
    
    /// <summary>
    /// 发送 Webhook POST 请求，不期望有响应内容。
    /// </summary>
    private async Task PostWebhookAsync<TRequest>(TRequest payload, CancellationToken cancellationToken)
    {
        await PostJsonAsync(WebhookUrl, payload, cancellationToken);
    }

    // --- 高层级 API (发送/无响应) ---

    /// <summary>
    /// 发送设备的位置更新信息。
    /// </summary>
    public Task SendLocationUpdateAsync(LocationUpdateRequest locationData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<LocationUpdateRequest>("update_location", locationData);
        return PostWebhookAsync(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 调用 Home Assistant 中的一个服务操作。
    /// </summary>
    public Task CallServiceActionAsync(CallServiceRequest serviceData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<CallServiceRequest>("call_service", serviceData);
        return PostWebhookAsync(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 触发 Home Assistant 中的一个事件。
    /// </summary>
    public Task FireEventAsync(FireEventRequest eventData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<FireEventRequest>("fire_event", eventData);
        return PostWebhookAsync(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 更新已注册设备的信息。
    /// </summary>
    public Task UpdateRegistrationAsync(UpdateRegistrationRequest updateData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<UpdateRegistrationRequest>("update_registration", updateData);
        return PostWebhookAsync(payload, cancellationToken);
    }
    
    /// <summary>
    /// 通过 Webhook 注册一个新的传感器或二进制传感器。
    /// </summary>
    public Task RegisterSensorAsync(RegisterSensorRequest sensorData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<RegisterSensorRequest>("register_sensor", sensorData);
        return PostWebhookAsync(payload, cancellationToken);
    }

    // --- 高层级 API (获取/有响应) ---

    /// <summary>
    /// 通过 Webhook 渲染一个或多个 Home Assistant 模板。
    /// </summary>
    public Task<Dictionary<string, string>?> RenderTemplatesAsync(Dictionary<string, TemplateData> templatesData, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<RenderTemplateRequest>("render_template", new RenderTemplateRequest(templatesData));
        return PostWebhookAsync<WebhookRequest<RenderTemplateRequest>, Dictionary<string, string>>(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 获取所有启用的区域（Zones）。
    /// </summary>
    public Task<List<object>?> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        var payload = new WebhookBaseRequest("get_zones");
        return PostWebhookAsync<WebhookBaseRequest, List<object>>(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 获取 Home Assistant 的配置信息。
    /// </summary>
    public Task<object?> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        var payload = new WebhookBaseRequest("get_config");
        return PostWebhookAsync<WebhookBaseRequest, object>(payload, cancellationToken);
    }

    /// <summary>
    /// 通过 Webhook 批量更新一个或多个已注册传感器的状态和属性。
    /// </summary>
    public Task<Dictionary<string, UpdateSensorResult>?> UpdateSensorsAsync(List<UpdateSensorRequest> updates, CancellationToken cancellationToken = default)
    {
        var payload = new WebhookRequest<List<UpdateSensorRequest>>("update_sensor_states", updates);
        return PostWebhookAsync<WebhookRequest<List<UpdateSensorRequest>>, Dictionary<string, UpdateSensorResult>>(payload, cancellationToken);
    }
}