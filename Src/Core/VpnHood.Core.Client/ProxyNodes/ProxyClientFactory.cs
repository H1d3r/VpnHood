﻿using System.Net;
using VpnHood.Core.Client.Abstractions.ProxyNodes;
using VpnHood.Core.Proxies;
using VpnHood.Core.Proxies.HttpProxyClients;
using VpnHood.Core.Proxies.Socks4ProxyClients;
using VpnHood.Core.Proxies.Socks5ProxyClients;
using VpnHood.Core.Toolkit.Utils;

namespace VpnHood.Core.Client.ProxyNodes;

public static class ProxyClientFactory
{
    public static async Task<IPAddress> GetIpAddress(string host)
    {
        // try parse the proxy address
        if (IPAddress.TryParse(host, out var ipAddress))
            return ipAddress;

        var entry = await Dns.GetHostEntryAsync(host).Vhc();
        if (entry.AddressList.Length == 0)
            throw new Exception("Failed to resolve proxy server address.");

        // select a random address if multiple addresses are returned
        ipAddress = entry.AddressList[Random.Shared.Next(entry.AddressList.Length)];
        return ipAddress;
    }

    public static async Task<IProxyClient> CreateProxyClient(ProxyNode proxyNode)
    {
        var serverIp = await GetIpAddress(proxyNode.Host).Vhc();
        var serverEp = new IPEndPoint(serverIp, proxyNode.Port);

        return proxyNode.Protocol switch {
            ProxyProtocol.Socks5 => new Socks5ProxyClient(new Socks5ProxyClientOptions {
                ProxyEndPoint = serverEp,
                Password = proxyNode.Password,
                Username = proxyNode.Username
            }),
            ProxyProtocol.Socks4 => new Socks4ProxyClient(new Socks4ProxyClientOptions {
                ProxyEndPoint = serverEp,
                UserName = proxyNode.Username
            }),
            ProxyProtocol.Https or ProxyProtocol.Http => new HttpProxyClient(new HttpProxyClientOptions {
                ProxyEndPoint = serverEp,
                Username = proxyNode.Username,
                Password = proxyNode.Password,
                AllowInvalidCertificates = true,
                ProxyHost = proxyNode.Host,
                UseTls = proxyNode.Protocol == ProxyProtocol.Https
            }),
            _ => throw new NotSupportedException($"Proxy type {proxyNode.Protocol} is not supported.")
        };
    }
}