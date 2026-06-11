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

            try
            {
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
            }

            return instances.ToList();
        }

        private static async Task DiscoverViaMDnsAsync(ConcurrentBag<HassInstance> instances, 
            ConcurrentHashSet<string> discoveredUrls, CancellationToken cancellationToken)
        {
            try
            {
                var localIps = GetLocalIpAddresses();
                if (localIps.Count == 0)
                {
                    Debug.WriteLine("[HassDiscovery] No local IP addresses found");
                    return;
                }

                var query = BuildMDnsQuery(MDnsServiceType);
                var tasks = new List<Task>();

                foreach (var localIp in localIps)
                {
                    tasks.Add(DiscoverOnInterfaceAsync(localIp, query, instances, discoveredUrls, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] mDNS discovery error: {ex.Message}");
            }
        }

        private static async Task DiscoverOnInterfaceAsync(IPAddress localIp, byte[] query,
            ConcurrentBag<HassInstance> instances, ConcurrentHashSet<string> discoveredUrls,
            CancellationToken cancellationToken)
        {
            try
            {
                using var udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                udpClient.MulticastLoopback = true;
                udpClient.Client.ReceiveTimeout = DiscoveryTimeoutMs;
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                udpClient.Client.Bind(new IPEndPoint(localIp, 0));
                udpClient.JoinMulticastGroup(IPAddress.Parse(MDnsMulticastAddress), localIp);

                await udpClient.SendAsync(query, query.Length, new IPEndPoint(IPAddress.Parse(MDnsMulticastAddress), MDnsPort));

                var timeoutToken = new CancellationTokenSource(DiscoveryTimeoutMs);
                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token).Token;

                while (!linkedToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync().WaitAsync(linkedToken);
                        ParseMDnsResponse(result.Buffer, result.RemoteEndPoint, instances, discoveredUrls);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] Interface {localIp} error: {ex.Message}");
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

            var tasks = hostnames.Select(hostname => TryResolveHostnameAsync(hostname, instances, discoveredUrls));
            await Task.WhenAll(tasks);
        }

        private static async Task TryResolveHostnameAsync(string hostname, 
            ConcurrentBag<HassInstance> instances, ConcurrentHashSet<string> discoveredUrls)
        {
            try
            {
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
                        Debug.WriteLine($"[HassDiscovery] Resolved hostname: {url}");
                    }
                }
            }
            catch
            {
                // Hostname resolution failed, ignore
            }
        }

        private static List<IPAddress> GetLocalIpAddresses()
        {
            var ips = new List<IPAddress>();

            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus != OperationalStatus.Up) continue;
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

                foreach (var addrInfo in iface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ips.Add(addrInfo.Address);
                    }
                }
            }

            return ips;
        }

        // Simple concurrent hash set implementation
        private class ConcurrentHashSet<T>
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
