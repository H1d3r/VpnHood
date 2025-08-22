﻿using System.Net;
using System.Text.Json.Serialization;
using VpnHood.Core.Client.Abstractions;
using VpnHood.Core.Common.Tokens;
using VpnHood.Core.Toolkit.Converters;

namespace VpnHood.AppLib.Settings;

public class UserSettings
{
    public bool IsLicenseAccepted { get; set; }
    public bool IsTcpProxyPrompted { get; set; }
    public bool IsQuickLaunchPrompted { get; set; }
    public string? CultureCode { get; set; }
    public Guid? ClientProfileId { get; set; }
    public int MaxPacketChannelCount { get; set; } = ClientOptions.Default.MaxPacketChannelCount;
    public bool TunnelClientCountry { get; set; } = true;
    public string[] AppFilters { get; set; } = [];
    public FilterMode AppFiltersMode { get; set; } = FilterMode.All;
    public bool UseTcpProxy { get; set; }
    public bool UseUdpChannel { get; set; } = ClientOptions.Default.UseUdpChannel;
    public bool DropUdp { get; set; } = ClientOptions.Default.DropUdp;
    public bool DropQuic { get; set; } = ClientOptions.Default.DropQuic;
    public bool AllowAnonymousTracker { get; set; } = ClientOptions.Default.AllowAnonymousTracker;
    public DomainFilter DomainFilter { get; set; } = new();
    public string? DebugData1 { get; set; }
    public string? DebugData2 { get; set; }
    public bool LogAnonymous { get; set; } = true;
    public bool IncludeLocalNetwork { get; set; } = ClientOptions.Default.IncludeLocalNetwork;
    public bool UseAppIpFilter { get; set; }
    public bool UseVpnAdapterIpFilter { get; set; }
    public EndPointStrategy EndPointStrategy { get; set; }
    
    [JsonConverter(typeof(ArrayConverter<IPAddress, IPAddressConverter>))]
    public IPAddress[]? DnsServers { get; set; }
    public bool UseProxyServer { get; set; }
    public ProxyServerEndPoint[] ProxyServers { get; set; } = [];
}