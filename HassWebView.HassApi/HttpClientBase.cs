using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HassWebView.HassApi
{
    /// <summary>
    /// Home Assistant API 的 HTTP 客户端基类，封装了认证和 JSON 序列化/反序列化。
    /// </summary>
    public abstract class HttpClientBase
    {
        // ----------------------------------------------------------------
        // 静态成员和实例字段
        // ----------------------------------------------------------------

        /// <summary>
        /// 用于所有 HTTP 请求的静态 JSON 序列化选项，配置为 snake_case 命名策略。
        /// </summary>
        protected static readonly JsonSerializerOptions SnakeCaseJsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };

        /// <summary>
        /// 封装的底层 HttpClient 实例。
        /// </summary>
        protected readonly HttpClient RawClient;

        /// <summary>
        /// 一个可选的回调函数，用于获取或刷新 AccessToken。
        /// bool 参数代表是否需要强制刷新 (例如，因为收到了 401 错误)。
        /// </summary>
        private readonly Func<bool, Task<string>>? _tokenRefreshCallback;

        private string? _accessToken;

        // ----------------------------------------------------------------
        // 构造函数和属性
        // ----------------------------------------------------------------

        protected HttpClientBase(string baseAddress, Func<bool, Task<string>>? tokenRefreshCallback = null)
        {
            RawClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            _tokenRefreshCallback = tokenRefreshCallback;
            AccessToken = null;
        }

        /// <summary>
        /// 获取或设置用于 API 请求的 Bearer 令牌 (Access Token)。
        /// 当设置新令牌时，会自动更新 HttpClient 的默认请求头。
        /// </summary>
        public string? AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;
                if (string.IsNullOrEmpty(_accessToken))
                {
                    RawClient.DefaultRequestHeaders.Authorization = null;
                }
                else
                {
                    RawClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                }
            }
        }

        // ----------------------------------------------------------------
        // 核心 HTTP 方法（Protected 封装）
        // ----------------------------------------------------------------

        protected async Task<T?> GetJsonAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class
        {
            var response = await ExecuteRequestAsync(() => RawClient.GetAsync(endpoint, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[API-GET] {endpoint} failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<T>(SnakeCaseJsonOptions, cancellationToken);
        }

        protected async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest payload, CancellationToken cancellationToken = default) where TResponse : class
        {
            var response = await ExecuteRequestAsync(() => RawClient.PostAsJsonAsync(endpoint, payload, SnakeCaseJsonOptions, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[API-POST] {endpoint} failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>(SnakeCaseJsonOptions, cancellationToken);
        }

        protected async Task<bool> PostJsonAsync<TRequest>(string endpoint, TRequest payload, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequestAsync(() => RawClient.PostAsJsonAsync(endpoint, payload, SnakeCaseJsonOptions, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[API-POST] {endpoint} failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                return false;
            }
            return true;
        }

        // ----------------------------------------------------------------
        // 私有核心逻辑: 自动令牌刷新和重试
        // ----------------------------------------------------------------

        /// <summary>
        /// 执行一个 HTTP 请求，并提供自动的令牌刷新和重试逻辑。
        /// </summary>
        /// <param name="requestFunc">一个返回 HttpResponseMessage 任务的委托，代表要执行的 HTTP 请求。</param>
        /// <returns>HTTP 响应消息。</returns>
        private async Task<HttpResponseMessage> ExecuteRequestAsync(Func<Task<HttpResponseMessage>> requestFunc)
        {
            // 如果我们还没有令牌，并且有办法获取它，就先获取一个。
            // 这不是强制刷新，因此传入 "false"。
            if (string.IsNullOrEmpty(AccessToken) && _tokenRefreshCallback != null)
            {
                var initialToken = await _tokenRefreshCallback(false);
                if (!string.IsNullOrEmpty(initialToken))
                {
                    this.AccessToken = initialToken;
                    Debug.WriteLine("[HttpClientBase] Initial token fetched successfully.");
                }
                else
                {
                    Debug.WriteLine("[HttpClientBase] Failed to fetch initial token.");
                }
            }

            var response = await requestFunc();

            // 如果响应是 401 Unauthorized 并且我们有一个刷新令牌的回调函数，
            // 这意味着令牌无效（过期或被撤销），必须强制刷新。
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && _tokenRefreshCallback != null)
            {
                Debug.WriteLine("[HttpClientBase] Received 401 Unauthorized. Forcing token refresh.");

                // 调用回调以获取新的访问令牌，传入 "true" 表示强制执行。
                var newAccessToken = await _tokenRefreshCallback(true);

                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    this.AccessToken = newAccessToken;
                    Debug.WriteLine("[HttpClientBase] Token refreshed. Retrying the original request.");

                    response.Dispose();
                    response = await requestFunc();
                }
                else
                {
                    Debug.WriteLine("[HttpClientBase] Token refresh callback failed or returned no token after 401.");
                }
            }

            return response;
        }
    }
}
