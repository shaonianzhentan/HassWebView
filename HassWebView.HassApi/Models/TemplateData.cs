using System.Collections.Generic;

namespace HassWebView.HassApi.Models;

/// <summary>
/// 单个模板渲染请求的具体内容。
/// </summary>
/// <param name="Template">必填：要渲染的 Jinja2 模板字符串。</param>
/// <param name="Variables">可选：模板渲染时可用的变量字典。</param>
public record TemplateData(
    string Template,
    Dictionary<string, object>? Variables
);