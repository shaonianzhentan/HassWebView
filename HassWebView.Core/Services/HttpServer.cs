
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#if ANDROID
using Android.Content;
using Android.Net;
#endif

namespace HassWebView.Core.Services
{
    public class HttpServer : IDisposable
    {
        public string? BaseUrl { get; private set; }
        public int Port { get; private set; }
        private readonly HttpListener _listener = new HttpListener();

        // 路由表
        private readonly Dictionary<string, Dictionary<string, Func<Request, Response, Task>>> _routes = new();

        public class Request
        {
            private readonly HttpListenerRequest _req;
            private string? _body;

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
                return JsonSerializer.Deserialize<T>(body, options)!;
            }
        }

        public class Response
        {
            private readonly HttpListenerResponse _res;

            public Response(HttpListenerResponse res) { _res = res; }
            
            public HttpListenerResponse OriginalResponse => _res;

            public async Task Json(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                var json = JsonSerializer.Serialize(data);
                _res.ContentType = "application/json";
                _res.StatusCode = (int)statusCode;
                var buf = Encoding.UTF8.GetBytes(json);
                _res.ContentLength64 = buf.Length;
                await _res.OutputStream.WriteAsync(buf, 0, buf.Length);
            }

            public async Task Text(string text, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _res.ContentType = "text/plain";
                _res.StatusCode = (int)statusCode;
                var buf = Encoding.UTF8.GetBytes(text);
                _res.ContentLength64 = buf.Length;
                await _res.OutputStream.WriteAsync(buf, 0, buf.Length);
            }

            public async Task Html(string html, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _res.ContentType = "text/html";
                _res.StatusCode = (int)statusCode;
                var buf = Encoding.UTF8.GetBytes(html);
                _res.ContentLength64 = buf.Length;
                await _res.OutputStream.WriteAsync(buf, 0, buf.Length);
            }

            public async Task FileAsync(string filePath, string? contentType = null)
            {
                if (!File.Exists(filePath))
                {
                    _res.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                _res.ContentType = contentType ?? GetMimeType(Path.GetExtension(filePath));
                var buf = await File.ReadAllBytesAsync(filePath);
                _res.ContentLength64 = buf.Length;
                await _res.OutputStream.WriteAsync(buf, 0, buf.Length);
            }
        }

        // ========== 构造函数 ==========

        /// <summary>
        /// 创建 HTTP 服务器（随机端口，智能选择 IP）
        /// </summary>
        public HttpServer() : this(0, true) { }

        /// <summary>
        /// 创建 HTTP 服务器
        /// </summary>
        public HttpServer(int port = 0, bool preferLan = true)
        {
            var actualPort = port == 0 ? GetAvailablePort() : port;
            Port = actualPort;

#if WINDOWS
            // 在 Windows 上只绑定 localhost（普通用户权限即可）
            BaseUrl = $"http://localhost:{actualPort}/";
            _listener.Prefixes.Add($"http://localhost:{actualPort}/");
            _listener.Prefixes.Add($"http://127.0.0.1:{actualPort}/");
            Debug.WriteLine($"[HttpServer] Bound to localhost on port {actualPort}");
#else
            var ip = preferLan ? GetBestBindAddress() : "localhost";
            if (string.IsNullOrEmpty(ip))
                ip = "localhost";
            BaseUrl = $"http://{ip}:{actualPort}/";
            _listener.Prefixes.Add($"http://+:{actualPort}/");
#endif
            Debug.WriteLine($"[HttpServer] Configured on {BaseUrl}");
            Debug.WriteLine("[HttpServer] Serving static files from embedded resources");
        }

        // ========== 路由 API ==========

        public void AddRoute(string method, string path, Func<Request, Response, Task> handler)
        {
            var pk = path.ToLower().TrimEnd('/');
            var mk = method.ToUpper();
            if (!_routes.ContainsKey(pk))
                _routes[pk] = new Dictionary<string, Func<Request, Response, Task>>();
            _routes[pk][mk] = handler;
        }

        public void Get(string path, Func<Request, Response, Task> handler) => AddRoute("GET", path, handler);
        public void Post(string path, Func<Request, Response, Task> handler) => AddRoute("POST", path, handler);
        public void Put(string path, Func<Request, Response, Task> handler) => AddRoute("PUT", path, handler);
        public void Delete(string path, Func<Request, Response, Task> handler) => AddRoute("DELETE", path, handler);

        /// <summary>
        /// 移除指定路由
        /// </summary>
        public void RemoveRoute(string method, string path)
        {
            var pk = path.ToLower().TrimEnd('/');
            var mk = method.ToUpper();
            if (_routes.TryGetValue(pk, out var mr))
            {
                mr.Remove(mk);
                if (mr.Count == 0)
                    _routes.Remove(pk);
            }
        }

        public void RemoveGet(string path) => RemoveRoute("GET", path);
        public void RemovePost(string path) => RemoveRoute("POST", path);
        public void RemovePut(string path) => RemoveRoute("PUT", path);
        public void RemoveDelete(string path) => RemoveRoute("DELETE", path);

        /// <summary>
        /// 生成二维码图片（Base64 PNG）
        /// </summary>
        public string GenerateQrCodeImage(string content, int size = 200)
        {
            try
            {
                var svg = QrCodeService.GenerateSvg(content, size, QrCodeService.ErrorCorrectionLevel.H);
                // 这里可以扩展为返回 PNG 格式，目前返回 SVG
                return svg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HttpServer] Error generating QR code: {ex.Message}");
                return string.Empty;
            }
        }

        // ========== 启动 / 停止 ==========

        public async Task StartAsync()
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("HttpListener is not supported.");

            try
            {
                _listener.Start();
                Debug.WriteLine($"[HttpServer] Listening on {BaseUrl}");

                while (_listener.IsListening)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        _ = Task.Run(() => HandleRequest(context));
                    }
                    catch (HttpListenerException ex)
                    {
                        if (!_listener.IsListening)
                        {
                            Debug.WriteLine($"[HttpServer] Stopped normally");
                            break;
                        }
                        Debug.WriteLine($"[HttpServer] Request error: {ex.Message} (Error code: {ex.ErrorCode})");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[HttpServer] Request processing error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HttpServer] Fatal error: {ex.Message}");
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            var req = new Request(context.Request);
            var res = new Response(context.Response);
            var rawPath = context.Request.Url?.AbsolutePath ?? "/";
            var path = rawPath.ToLower().TrimEnd('/');
            var method = context.Request.HttpMethod.ToUpper();

            // CORS
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            try
            {
                if (method == "OPTIONS")
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    context.Response.OutputStream.Close();
                    return;
                }

                // 1. 优先匹配路由
                if (_routes.TryGetValue(path, out var mr) && mr.TryGetValue(method, out var h))
                {
                    await h(req, res);
                }
                // 2. 尝试静态文件（从嵌入式资源读取）
                else if (TryServeStaticFile(rawPath, context))
                {
                    // 文件已发送，无需额外处理
                }
                // 3. 默认首页（index.html）
                else if (TryServeDefaultPage(context))
                {
                    // 默认页已发送
                }
                else
                {
                    await res.Text("404 Not Found", HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                try { await res.Text(ex.Message, HttpStatusCode.InternalServerError); } catch { }
            }
            finally
            {
                try { context.Response.OutputStream.Close(); } catch { }
            }
        }

        // ========== 静态文件服务 ==========

        private bool TryServeStaticFile(string rawPath, HttpListenerContext context)
        {
            // 安全校验：防止路径遍历
            var relativePath = rawPath.TrimStart('/').Replace('/', '.');
            
            // 构建嵌入式资源名称（格式：Namespace.Folder.File）
            var resourceName = $"HassWebView.Core.Resources.wwwroot.{relativePath}";
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return false;

                var ext = Path.GetExtension(rawPath);
                context.Response.ContentType = GetMimeType(ext);
                stream.CopyTo(context.Response.OutputStream);
                return true;
            }
        }

        private bool TryServeDefaultPage(HttpListenerContext context)
        {
            // 按优先级查找默认页（从嵌入式资源读取）
            var defaultPages = new[] { "index.html", "index.htm" };
            foreach (var page in defaultPages)
            {
                if (TryServeStaticFile(page, context))
                    return true;
            }
            return false;
        }

        private static string GetMimeType(string ext) => ext.ToLower() switch
        {
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            _ => "application/octet-stream"
        };

        public void Stop()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
                Debug.WriteLine("[HttpServer] Stopped");
            }
        }

        public void Dispose() => Stop();

        // ========== 网络状态 & IP 选择 ==========

        public static string GetBestBindAddress()
        {
            try
            {
                var activeConnection = GetActiveConnectionType();
                if (activeConnection == ConnectionType.Mobile)
                {
                    Debug.WriteLine("[HttpServer] Mobile network detected, binding to localhost only");
                    return "localhost";
                }

                var ip = GetLocalIPv4Address();
                return string.IsNullOrEmpty(ip) ? "localhost" : ip;
            }
            catch
            {
                return "localhost";
            }
        }

        public static ConnectionType GetActiveConnectionType()
        {
#if ANDROID
            try
            {
                var cm = Android.App.Application.Context.GetSystemService(Context.ConnectivityService) as Android.Net.ConnectivityManager;
                if (cm != null)
                {
                    var active = cm.ActiveNetworkInfo;
                    if (active != null && active.IsConnected)
                    {
                        return active.Type switch
                        {
                            Android.Net.ConnectivityType.Wifi => ConnectionType.WiFi,
                            Android.Net.ConnectivityType.Mobile => ConnectionType.Mobile,
                            Android.Net.ConnectivityType.Ethernet => ConnectionType.Ethernet,
                            _ => ConnectionType.Unknown
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HttpServer] Failed to get connection type: {ex.Message}");
            }
#endif
            try
            {
                var hasWifiOrEthernet = NetworkInterface.GetAllNetworkInterfaces()
                    .Any(ni => ni.OperationalStatus == OperationalStatus.Up &&
                               (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet));
                return hasWifiOrEthernet ? ConnectionType.WiFi : ConnectionType.Unknown;
            }
            catch
            {
                return ConnectionType.Unknown;
            }
        }

        public static string GetLocalIPv4Address()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                                 !ni.Description.ToLower().Contains("virtual") &&
                                 !ni.Description.ToLower().Contains("pseudo"))
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ua => ua.Address)
                    .OrderByDescending(ip =>
                    {
                        var bytes = ip.GetAddressBytes();
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

        public static int GetAvailablePort()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                if (socket.LocalEndPoint is IPEndPoint ep)
                    return ep.Port;
            }
            catch { }
            return new Random().Next(50000, 65535);
        }

        public enum ConnectionType
        {
            Unknown,
            WiFi,
            Mobile,
            Ethernet
        }
    }
}
