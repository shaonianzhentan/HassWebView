using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace HassWebView.HassApi;

public abstract class HttpClientBase
{
    public readonly string baseUrl;
    protected readonly HttpClient RawClient;

    protected static readonly JsonSerializerOptions SnakeCaseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Func<bool, Task<string>>? _tokenRefreshCallback;
    private string? _accessToken;

    protected HttpClientBase(string baseAddress, Func<bool, Task<string>>? tokenRefreshCallback = null)
    {
        // 1. Parse the potentially complex input address.
        var inputUri = new Uri(baseAddress);

        // 2. Extract only the scheme and authority to create a clean base URI.
        //    This correctly handles cases where the input has a path or query.
        //    The new Uri() constructor ensures it ends with a '/' for correct relative path joining.
        var cleanBaseUri = new Uri(inputUri.GetLeftPart(UriPartial.Authority));

        // 3. Assign the clean URI parts.
        this.baseUrl = cleanBaseUri.ToString();
        RawClient = new HttpClient
        {
            BaseAddress = cleanBaseUri
        };
        
        _tokenRefreshCallback = tokenRefreshCallback;
        AccessToken = null;
    }

    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            _accessToken = value;
            if (!string.IsNullOrEmpty(_accessToken))
            {
                RawClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            else
            {
                RawClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }

    protected async Task<T?> GetJsonAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequestAsync(() => RawClient.GetAsync(endpoint, cancellationToken));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>(SnakeCaseJsonOptions, cancellationToken) : default;
    }

    protected async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest? payload, CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequestAsync(() => RawClient.PostAsJsonAsync(endpoint, payload, SnakeCaseJsonOptions, cancellationToken));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>(SnakeCaseJsonOptions, cancellationToken) : default;
    }

    protected async Task<HttpResponseMessage> ExecuteRequestAsync(Func<Task<HttpResponseMessage>> apiCall)
    {
        if (_tokenRefreshCallback != null && string.IsNullOrEmpty(AccessToken))
        {
            var token = await _tokenRefreshCallback(false);
            if (string.IsNullOrEmpty(token))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Authentication token could not be obtained." };
            }
            AccessToken = token;
        }

        var response = await apiCall();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && _tokenRefreshCallback != null)
        {
            var newToken = await _tokenRefreshCallback(true);
            if (string.IsNullOrEmpty(newToken))
            {
                return response; 
            }
            AccessToken = newToken;
            response = await apiCall();
        }

        return response;
    }
}
