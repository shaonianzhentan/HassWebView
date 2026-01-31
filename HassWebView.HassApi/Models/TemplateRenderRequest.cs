namespace HassWebView.HassApi.Models;

/// <summary>
/// Home Assistant 模板渲染请求体模型。
/// 对应 POST /api/template 接口的请求 JSON Payload。
/// </summary>
public record TemplateRenderRequest
{
    /// <summary>
    /// 要渲染的 Home Assistant 模板字符串。
    /// 对应 JSON 中的 "template"。
    /// </summary>
    public required string Template { get; init; }
}