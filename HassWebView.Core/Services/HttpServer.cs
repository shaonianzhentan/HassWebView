
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HassWebView.Core.Services
{
    public class HttpServer : IDisposable
    {
        public string BaseUrl { get; private set; }
        private readonly HttpListener _listener = new HttpListener();

        public class Request
        {
            private readonly HttpListenerRequest _req;
            private string _body;

            public NameValueCollection Query { get; }
            public HttpListenerRequest OriginalRequest => _req;

            public Request(HttpListenerRequest req)
            {
                _req = req;
                Query = req.QueryString;
            }

            public async Task<string> BodyAsync()
            {
                if (_body == null)
                {
                    using var reader = new StreamReader(_req.InputStream, _req.ContentEncoding);
                    _body = await reader.ReadToEndAsync();
                }
                return _body;
            }

            public async Task<T> JsonAsync<T>()
            {
                var body = await BodyAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<T>(body, options);
            }
        }

        public class Response
        {
            private readonly HttpListenerResponse _res;

            public Response(HttpListenerResponse res)
            {
                _res = res;
            }

            public async Task Json(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                var json = JsonSerializer.Serialize(data);
                _res.ContentType = "application/json";
                _res.StatusCode = (int)statusCode;
                var buffer = Encoding.UTF8.GetBytes(json);
                _res.ContentLength64 = buffer.Length;
                await _res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            public async Task Text(string text, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _res.ContentType = "text/plain";
                _res.StatusCode = (int)statusCode;
                var buffer = Encoding.UTF8.GetBytes(text);
                _res.ContentLength64 = buffer.Length;
                await _res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            public async Task Html(string html, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _res.ContentType = "text/html";
                _res.StatusCode = (int)statusCode;
                var buffer = Encoding.UTF8.GetBytes(html);
                _res.ContentLength64 = buffer.Length;
                await _res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private readonly Dictionary<string, Dictionary<string, Func<Request, Response, Task>>> _routes =
            new Dictionary<string, Dictionary<string, Func<Request, Response, Task>>>();

        public HttpServer(string ip, int port)
        {
            BaseUrl = $"http://{ip}:{port}/";
#if WINDOWS
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
#else
            _listener.Prefixes.Add($"http://+:{port}/");
#endif
        }

        public void AddRoute(string method, string path, Func<Request, Response, Task> handler)
        {
            var pathKey = path.ToLower();
            var methodKey = method.ToUpper();

            if (!_routes.ContainsKey(pathKey)) _routes[pathKey] = new Dictionary<string, Func<Request, Response, Task>>();
            
            _routes[pathKey][methodKey] = handler;
        }

        public void Get(string path, Func<Request, Response, Task> handler) => AddRoute("GET", path, handler);
        public void Post(string path, Func<Request, Response, Task> handler) => AddRoute("POST", path, handler);
        public void Put(string path, Func<Request, Response, Task> handler) => AddRoute("PUT", path, handler);
        public void Delete(string path, Func<Request, Response, Task> handler) => AddRoute("DELETE", path, handler);

        public async Task StartAsync()
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("HttpListener is not supported.");
            
            try
            {
                _listener.Start();
                Console.WriteLine($"Listening on {_listener.Prefixes.First()}...");

                while (_listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    await RouteRequest(context);
                }
            }
            catch (HttpListenerException ex) when (_listener.IsListening)
            {
                Console.WriteLine($"HttpListenerException: {ex.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task RouteRequest(HttpListenerContext context)
        {
            var request = new Request(context.Request);
            var response = new Response(context.Response);
            var path = context.Request.Url.AbsolutePath.ToLower();
            var method = context.Request.HttpMethod.ToUpper();

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (method == "OPTIONS")
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                context.Response.OutputStream.Close();
                return;
            }

            try
            {
                if (_routes.TryGetValue(path, out var methodRoutes) && methodRoutes.TryGetValue(method, out var handler))
                {
                    await handler(request, response);
                }
                else
                {
                    var allowedMethods = _routes.ContainsKey(path) ? string.Join(", ", _routes[path].Keys) : "None";
                    await response.Text($"404 Not Found or Method Not Allowed. Allowed: {allowedMethods}", HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await response.Text(ex.Message, HttpStatusCode.InternalServerError);
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Failed to send error response: {innerEx.Message}");
                }
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        public void Stop() { if (_listener.IsListening) { _listener.Stop(); _listener.Close(); } }
        public void Dispose() => Stop();

        public static string GetLocalIPv4Address()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 !ni.Description.ToLower().Contains("virtual") &&
                                 !ni.Description.ToLower().Contains("pseudo"))
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ua => ua.Address)
                    .OrderByDescending(ip =>
                    {
                        byte[] bytes = ip.GetAddressBytes();
                        return bytes[0] switch
                        {
                            10 => 3,
                            172 when bytes[1] >= 16 && bytes[1] <= 31 => 2,
                            192 when bytes[1] == 168 => 1,
                            _ => 0
                        };
                    })
                    .FirstOrDefault()?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
