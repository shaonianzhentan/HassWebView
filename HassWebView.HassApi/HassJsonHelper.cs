using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace HassWebView.HassApi;

/// <summary>
/// Home Assistant 客户端专用的 JSON 序列化和反序列化工具类。
/// 封装了 Home Assistant API 所需的特定 JsonSerializerOptions (例如 SnakeCase 策略)。
/// </summary>
public static class HassJsonHelper
{
    // 关键修改: 封装 Home Assistant API 专用的 JSON 配置
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };
    
    /// <summary>
    /// 使用 Home Assistant 专用的配置序列化一个对象到 JSON 字符串。
    /// </summary>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    /// <summary>
    /// 使用 Home Assistant 专用的配置将 JSON 字符串反序列化为指定类型。
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return default;
        // 注意：这里使用 T! 来帮助编译器处理 nullability，假设调用者处理可能的 null 返回。
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// 异步地从流中反序列化对象。
    /// </summary>
    public static ValueTask<T?> DeserializeAsync<T>(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<T>(utf8Json, Options, cancellationToken);
    }
}