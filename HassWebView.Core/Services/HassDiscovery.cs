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

            try
            {
                var tasks = new List<Task>
                {
                    DiscoverViaMDnsAsync(instances, cancellationToken),
                    DiscoverViaCommonHostnamesAsync(instances)
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] Discovery error: {ex.Message}");
            }

            return instances
                .GroupBy(i => i.Url)
                .Select(g => g.First())
                .ToList();
        }

        private static async Task DiscoverViaMDnsAsync(ConcurrentBag<HassInstance> instances, CancellationToken cancellationToken)
        {
            try
            {
                using var udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                udpClient.MulticastLoopback = true;
                udpClient.Client.ReceiveTimeout = DiscoveryTimeoutMs;

                var query = BuildMDnsQuery(MDnsServiceType);
                
                var localIps = GetLocalIpAddresses();
                foreach (var ip in localIps)
                {
                    try
                    {
                        udpClient.Client.Bind(new IPEndPoint(ip, 0));
                        udpClient.JoinMulticastGroup(IPAddress.Parse(MDnsMulticastAddress));
                        await udpClient.SendAsync(query, query.Length, new IPEndPoint(IPAddress.Parse(MDnsMulticastAddress), MDnsPort));
                    }
                    catch
                    {
                        // 忽略绑定错误
                    }
                }

                var receiveTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = await udpClient.ReceiveAsync();
                            ParseMDnsResponse(result.Buffer, result.RemoteEndPoint, instances);
                        }
                        catch (SocketException)
                        {
                            break;
                        }
                    }
                }, cancellationToken);

                await Task.WhenAny(receiveTask, Task.Delay(DiscoveryTimeoutMs, cancellationToken));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] mDNS discovery error: {ex.Message}");
            }
        }

        private static byte[] BuildMDnsQuery(string serviceType)
        {
            var sb = new StringBuilder();
            
            sb.Append("\x00\x00");           // Transaction ID
            sb.Append("\x00\x00");           // Flags
            sb.Append("\x00\x01");           // Questions: 1
            sb.Append("\x00\x00");           // Answer RRs: 0
            sb.Append("\x00\x00");           // Authority RRs: 0
            sb.Append("\x00\x00");           // Additional RRs: 0

            var parts = serviceType.Split('.');
            foreach (var part in parts)
            {
                sb.Append((char)part.Length);
                sb.Append(part);
            }
            sb.Append("\x00");               // End of QNAME
            sb.Append("\x00\x0C");           // QTYPE: PTR (12)
            sb.Append("\x00\x01");           // QCLASS: IN (1)

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static void ParseMDnsResponse(byte[] response, IPEndPoint remoteEndPoint, ConcurrentBag<HassInstance> instances)
        {
            try
            {
                int offset = 12;
                
                int questions = (response[4] << 8) | response[5];
                offset += questions * 256;

                int answers = (response[6] << 8) | response[7];
                
                for (int i = 0; i < answers; i++)
                {
                    if ((response[offset] & 0xC0) == 0xC0)
                    {
                        offset += 2;
                    }
                    else
                    {
                        while (response[offset] != 0)
                        {
                            offset += response[offset] + 1;
                        }
                        offset++;
                    }

                    offset += 4;
                    offset += 4;
                    
                    int rdLength = (response[offset] << 8) | response[offset + 1];
                    offset += 2;
                    
                    if (offset + rdLength <= response.Length)
                    {
                        var recordType = (response[offset - 6] << 8) | response[offset - 5];
                        
                        if (recordType == 33 && rdLength >= 6)
                        {
                            int port = (response[offset + 4] << 8) | response[offset + 5];
                            offset += 6;
                            
                            string hostname = ParseName(response, ref offset);
                            
                            if (!string.IsNullOrEmpty(hostname))
                            {
                                instances.Add(new HassInstance
                                {
                                    Name = "Home Assistant",
                                    HostName = hostname.EndsWith(".local") ? hostname.Substring(0, hostname.Length - 6) : hostname,
                                    Port = port
                                });
                            }
                        }
                        else
                        {
                            offset += rdLength;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HassDiscovery] mDNS parse error: {ex.Message}");
            }
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
                
                int length = data[offset];
                offset++;
                
                if (length > 0 && offset + length <= data.Length)
                {
                    if (!first) sb.Append('.');
                    sb.Append(Encoding.ASCII.GetString(data, offset, length));
                    offset += length;
                    first = false;
                }
            }
            
            if (data[offset] == 0)
            {
                offset++;
            }
            
            return sb.ToString();
        }

        private static Task DiscoverViaCommonHostnamesAsync(ConcurrentBag<HassInstance> instances)
        {
            var hostnames = new[]
            {
                "home-assistant.local",
                "hass.local",
                "hassio.local",
                "homeassistant.local",
                "hassos.local"
            };

            foreach (var hostname in hostnames)
            {
                instances.Add(new HassInstance
                {
                    Name = "Home Assistant",
                    HostName = hostname,
                    Port = DefaultHassPort
                });
            }

            return Task.CompletedTask;
        }

        private static List<IPAddress> GetLocalIpAddresses()
        {
            var ips = new List<IPAddress>();
            
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus != OperationalStatus.Up) continue;
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                
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
    }
}