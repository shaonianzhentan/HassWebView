using HassWebView.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace HassWebView.Core.Services
{
    public static class HassDiscovery
    {
        private const string MDnsServiceType = "_home-assistant._tcp.local";
        private const string MDnsMulticastAddress = "224.0.0.251";
        private const int MDnsPort = 5353;
        private const int DefaultHassPort = 8123;
        private const int DiscoveryTimeoutMs = 3000;

        public static async Task<List<HassInstance>> DiscoverAsync(CancellationToken cancellationToken = default)
        {
            var instances = new ConcurrentBag<HassInstance>();
            var discoveredUrls = new ConcurrentHashSet<string>();

            Debug.WriteLine("[HassDiscovery] Starting discovery...");

            try
            {
                var localIps = GetLocalIpAddresses();
                Debug.WriteLine($"[HassDiscovery] Found {localIps.Count} local IP addresses");
                foreach (var ip in localIps)
                {
                    Debug.WriteLine($"[HassDiscovery]   - {ip}");
                }

                var tasks = new List<Task>
                {
                    DiscoverViaMDnsAsync(instances, discoveredUrls, cancellationToken),
                    DiscoverViaCommonHostnamesAsync(instances, discoveredUrls)
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] Discovery error: {ex.Message}");
                Debug.WriteLine($"[HassDiscovery] Stack trace: {ex.StackTrace}");
            }

            Debug.WriteLine($"[HassDiscovery] Discovery completed, found {instances.Count} instances");
            return instances.ToList();
        }

        private static async Task DiscoverViaMDnsAsync(ConcurrentBag<HassInstance> instances, 
            ConcurrentHashSet<string> discoveredUrls, CancellationToken cancellationToken)
        {
            try
            {
                var localIps = GetLocalIpAddresses();
                Debug.WriteLine($"[HassDiscovery.mDNS] Starting mDNS discovery on {localIps.Count} interfaces");
                
                if (localIps.Count == 0)
                {
                    Debug.WriteLine("[HassDiscovery.mDNS] No local IP addresses found");
                    return;
                }

                var query = BuildMDnsQuery(MDnsServiceType);
                Debug.WriteLine($"[HassDiscovery.mDNS] Query built, size: {query.Length} bytes");
                
                var tasks = new List<Task>();

                foreach (var localIp in localIps)
                {
                    Debug.WriteLine($"[HassDiscovery.mDNS] Starting discovery on interface {localIp}");
                    tasks.Add(DiscoverOnInterfaceAsync(localIp, query, instances, discoveredUrls, cancellationToken));
                }

                await Task.WhenAll(tasks);
                Debug.WriteLine($"[HassDiscovery.mDNS] mDNS discovery completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery.mDNS] mDNS discovery error: {ex.Message}");
                Debug.WriteLine($"[HassDiscovery.mDNS] Stack trace: {ex.StackTrace}");
            }
        }

        private static async Task DiscoverOnInterfaceAsync(IPAddress localIp, byte[] query,
            ConcurrentBag<HassInstance> instances, ConcurrentHashSet<string> discoveredUrls,
            CancellationToken cancellationToken)
        {
            UdpClient? udpClient = null;
            try
            {
                Debug.WriteLine($"[HassDiscovery.Interface] Setting up UDP client on {localIp}");
                
                udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                udpClient.MulticastLoopback = true;
                udpClient.Client.ReceiveTimeout = DiscoveryTimeoutMs;
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                Debug.WriteLine($"[HassDiscovery.Interface] Binding to {localIp}");
                udpClient.Client.Bind(new IPEndPoint(localIp, 0));
                
                Debug.WriteLine($"[HassDiscovery.Interface] Joining multicast group {MDnsMulticastAddress}");
                try
                {
                    udpClient.JoinMulticastGroup(IPAddress.Parse(MDnsMulticastAddress), localIp);
                    Debug.WriteLine($"[HassDiscovery.Interface] Successfully joined multicast group");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HassDiscovery.Interface] Failed to join multicast group: {ex.Message}");
                }

                Debug.WriteLine($"[HassDiscovery.Interface] Sending mDNS query to {MDnsMulticastAddress}:{MDnsPort}");
                try
                {
                    var sentBytes = await udpClient.SendAsync(query, query.Length, new IPEndPoint(IPAddress.Parse(MDnsMulticastAddress), MDnsPort));
                    Debug.WriteLine($"[HassDiscovery.Interface] Query sent successfully, {sentBytes} bytes");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HassDiscovery.Interface] Failed to send query: {ex.Message}");
                    return;
                }

                var timeoutToken = new CancellationTokenSource(DiscoveryTimeoutMs);
                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token).Token;

                int responseCount = 0;
                while (!linkedToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync().WaitAsync(linkedToken);
                        responseCount++;
                        Debug.WriteLine($"[HassDiscovery.Interface] Received response #{responseCount} from {result.RemoteEndPoint}");
                        Debug.WriteLine($"[HassDiscovery.Interface] Response size: {result.Buffer.Length} bytes");
                        ParseMDnsResponse(result.Buffer, result.RemoteEndPoint, instances, discoveredUrls);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine($"[HassDiscovery.Interface] Receive cancelled");
                        break;
                    }
                    catch (SocketException ex)
                    {
                        Debug.WriteLine($"[HassDiscovery.Interface] Socket error: {ex.Message} (ErrorCode: {ex.ErrorCode})");
                        break;
                    }
                }
                
                Debug.WriteLine($"[HassDiscovery.Interface] Interface scan completed, received {responseCount} responses");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery.Interface] {localIp} error: {ex.Message}");
                Debug.WriteLine($"[HassDiscovery.Interface] Stack trace: {ex.StackTrace}");
            }
            finally
            {
                udpClient?.Dispose();
            }
        }

        private static byte[] BuildMDnsQuery(string serviceType)
        {
            using var stream = new MemoryStream();
            
            // Transaction ID (2 bytes)
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            
            // Flags (2 bytes) - standard query
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            
            // Questions: 1 (2 bytes)
            stream.WriteByte(0x00);
            stream.WriteByte(0x01);
            
            // Answer RRs: 0 (2 bytes)
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            
            // Authority RRs: 0 (2 bytes)
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            
            // Additional RRs: 0 (2 bytes)
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);

            // QNAME
            var parts = serviceType.Split('.');
            foreach (var part in parts)
            {
                stream.WriteByte((byte)part.Length);
                var partBytes = Encoding.ASCII.GetBytes(part);
                stream.Write(partBytes, 0, partBytes.Length);
            }
            stream.WriteByte(0x00); // End of QNAME
            
            // QTYPE: PTR (12)
            stream.WriteByte(0x00);
            stream.WriteByte(0x0C);
            
            // QCLASS: IN (1)
            stream.WriteByte(0x00);
            stream.WriteByte(0x01);

            return stream.ToArray();
        }

        private static void ParseMDnsResponse(byte[] response, IPEndPoint remoteEndPoint,
            ConcurrentBag<HassInstance> instances, ConcurrentHashSet<string> discoveredUrls)
        {
            try
            {
                if (response.Length < 12)
                    return;

                int offset = 12;

                // Parse questions
                int questions = (response[4] << 8) | response[5];
                for (int i = 0; i < questions; i++)
                {
                    while (offset < response.Length && response[offset] != 0)
                    {
                        int len = response[offset];
                        if ((len & 0xC0) == 0xC0)
                        {
                            offset += 2;
                            break;
                        }
                        offset += len + 1;
                    }
                    offset++; // Skip null terminator
                    offset += 4; // Skip QTYPE and QCLASS
                }

                // Parse answers
                int answers = (response[6] << 8) | response[7];
                for (int i = 0; i < answers; i++)
                {
                    offset = SkipName(response, offset);
                    
                    if (offset + 10 > response.Length)
                        break;

                    int type = (response[offset] << 8) | response[offset + 1];
                    int class_ = (response[offset + 2] << 8) | response[offset + 3];
                    int ttl = (response[offset + 4] << 24) | (response[offset + 5] << 16) | 
                              (response[offset + 6] << 8) | response[offset + 7];
                    int rdLength = (response[offset + 8] << 8) | response[offset + 9];
                    offset += 10;

                    if (offset + rdLength > response.Length)
                        break;

                    if (type == 33 && class_ == 1) // SRV record
                    {
                        ParseSrvRecord(response, ref offset, rdLength, remoteEndPoint, instances, discoveredUrls);
                    }
                    else if (type == 12 && class_ == 1) // PTR record - skip, we're looking for SRV
                    {
                        offset += rdLength;
                    }
                    else
                    {
                        offset += rdLength;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] mDNS parse error: {ex.Message}");
            }
        }

        private static int SkipName(byte[] data, int offset)
        {
            while (offset < data.Length && data[offset] != 0)
            {
                if ((data[offset] & 0xC0) == 0xC0)
                {
                    return offset + 2;
                }
                int len = data[offset];
                offset += len + 1;
            }
            return offset + 1;
        }

        private static string ParseName(byte[] data, ref int offset)
        {
            var sb = new StringBuilder();
            bool first = true;

            while (offset < data.Length && data[offset] != 0)
            {
                if ((data[offset] & 0xC0) == 0xC0)
                {
                    int pointer = ((data[offset] & 0x3F) << 8) | data[offset + 1];
                    int savedOffset = offset;
                    offset = pointer;
                    sb.Append(ParseName(data, ref offset));
                    offset = savedOffset + 2;
                    break;
                }

                int len = data[offset];
                offset++;

                if (len > 0 && offset + len <= data.Length)
                {
                    if (!first) sb.Append('.');
                    sb.Append(Encoding.ASCII.GetString(data, offset, len));
                    offset += len;
                    first = false;
                }
            }

            if (offset < data.Length && data[offset] == 0)
            {
                offset++;
            }

            return sb.ToString();
        }

        private static void ParseSrvRecord(byte[] data, ref int offset, int rdLength,
            IPEndPoint remoteEndPoint, ConcurrentBag<HassInstance> instances, 
            ConcurrentHashSet<string> discoveredUrls)
        {
            try
            {
                if (offset + rdLength > data.Length)
                    return;

                // Priority (2), Weight (2), Port (2)
                int priority = (data[offset] << 8) | data[offset + 1];
                int weight = (data[offset + 2] << 8) | data[offset + 3];
                int port = (data[offset + 4] << 8) | data[offset + 5];
                offset += 6;

                // Target hostname
                string hostname = ParseName(data, ref offset);
                
                if (string.IsNullOrEmpty(hostname))
                    return;

                // Clean up hostname
                if (hostname.EndsWith(".local"))
                {
                    hostname = hostname.Substring(0, hostname.Length - 6);
                }

                // Validate port
                if (port <= 0 || port > 65535)
                    port = DefaultHassPort;

                string url = $"http://{hostname}:{port}";
                
                if (discoveredUrls.TryAdd(url))
                {
                    instances.Add(new HassInstance
                    {
                        Name = "Home Assistant",
                        HostName = hostname,
                        Port = port
                    });
                    Debug.WriteLine($"[HassDiscovery] Found instance: {url}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] SRV parse error: {ex.Message}");
            }
        }

        private static async Task DiscoverViaCommonHostnamesAsync(ConcurrentBag<HassInstance> instances,
            ConcurrentHashSet<string> discoveredUrls)
        {
            var hostnames = new[]
            {
                "home-assistant.local",
                "hass.local",
                "hassio.local",
                "homeassistant.local",
                "hassos.local"
            };

            Debug.WriteLine("[HassDiscovery.Hostname] Starting hostname resolution");
            var tasks = hostnames.Select(hostname => TryResolveHostnameAsync(hostname, instances, discoveredUrls)).ToList();
            await Task.WhenAll(tasks);
            Debug.WriteLine("[HassDiscovery.Hostname] Hostname resolution completed");
        }

        private static async Task TryResolveHostnameAsync(string hostname, 
            ConcurrentBag<HassInstance> instances, ConcurrentHashSet<string> discoveredUrls)
        {
            try
            {
                Debug.WriteLine($"[HassDiscovery.Hostname] Resolving {hostname}...");
                var result = await Dns.GetHostEntryAsync(hostname);
                if (result.AddressList.Length > 0)
                {
                    string url = $"http://{hostname}:{DefaultHassPort}";
                    
                    if (discoveredUrls.TryAdd(url))
                    {
                        instances.Add(new HassInstance
                        {
                            Name = "Home Assistant",
                            HostName = hostname,
                            Port = DefaultHassPort
                        });
                        Debug.WriteLine($"[HassDiscovery.Hostname] Resolved hostname: {url}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[HassDiscovery.Hostname] {hostname} resolved but no IP addresses found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery.Hostname] {hostname} resolution failed: {ex.Message}");
            }
        }

        private static List<IPAddress> GetLocalIpAddresses()
        {
            var ips = new List<IPAddress>();
            Debug.WriteLine("[HassDiscovery.Network] Scanning network interfaces...");

            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.WriteLine($"[HassDiscovery.Network] Interface: {iface.Name} ({iface.Description})");
                Debug.WriteLine($"  Status: {iface.OperationalStatus}, Type: {iface.NetworkInterfaceType}");
                
                if (iface.OperationalStatus != OperationalStatus.Up)
                {
                    Debug.WriteLine($"  Skipping: Interface is down");
                    continue;
                }
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    Debug.WriteLine($"  Skipping: Loopback interface");
                    continue;
                }
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    Debug.WriteLine($"  Skipping: Tunnel interface");
                    continue;
                }

                foreach (var addrInfo in iface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Debug.WriteLine($"  Adding IPv4: {addrInfo.Address}");
                        ips.Add(addrInfo.Address);
                    }
                }
            }

            Debug.WriteLine($"[HassDiscovery.Network] Found {ips.Count} usable IPv4 addresses");
            return ips;
        }

        // Simple concurrent hash set implementation
        private class ConcurrentHashSet<T> where T : notnull
        {
            private readonly ConcurrentDictionary<T, byte> _dict = new ConcurrentDictionary<T, byte>();

            public bool TryAdd(T item)
            {
                return _dict.TryAdd(item, 0);
            }

            public bool Contains(T item)
            {
                return _dict.ContainsKey(item);
            }
        }
    }
}
