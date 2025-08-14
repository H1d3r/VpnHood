﻿using VpnHood.Core.Client.Device.UiContexts;

namespace VpnHood.Core.Client.Device;

public interface IDevice : IDisposable
{
    string VpnServiceConfigFolder { get; }
    bool IsTv { get; }
    bool IsExcludeAppsSupported { get; }
    bool IsIncludeAppsSupported { get; }
    bool IsBindProcessToVpnSupported { get; }
    bool IsTcpProxySupported { get; }
    string OsInfo { get; }
    DeviceMemInfo? MemInfo { get; }
    DeviceAppInfo[] InstalledApps { get; }
    Task RequestVpnService(IUiContext? uiContext, TimeSpan timeout, CancellationToken cancellationToken);
    Task StartVpnService(CancellationToken cancellationToken);
    void BindProcessToVpn(bool value);
}