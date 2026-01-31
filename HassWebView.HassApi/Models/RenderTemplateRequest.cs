using System.Collections.Generic;

namespace HassWebView.HassApi.Models;


/// <summary>
/// Webhook 消息中用于渲染模板的顶级 'data' 负载。
/// 它包含一个或多个命名模板的字典。
/// </summary>
/// <param name="Templates">必填：键值对字典，其中键是模板的名称，值是模板的具体内容（TemplateData）。</param>
public record RenderTemplateRequest(
    Dictionary<string, TemplateData> Templates
);