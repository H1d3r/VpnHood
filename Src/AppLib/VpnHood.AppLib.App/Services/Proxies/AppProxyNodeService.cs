﻿using VpnHood.AppLib.Settings;
using VpnHood.Core.Client.Abstractions.ProxyNodes;
using VpnHood.Core.Client.VpnServices.Manager;
using VpnHood.Core.Common.IpLocations;
using VpnHood.Core.Toolkit.Utils;

namespace VpnHood.AppLib.Services.Proxies;

public class AppProxyNodeService(
    string storageFolder,
    IIpLocationProvider? ipLocationProvider,
    VpnServiceManager vpnServiceManager,
    AppSettingsService settingsService)
{
    private ServiceData? _data;
    private readonly string _infoFilePath = Path.Combine(storageFolder, "proxy_infos.json");
    private AppProxySettings ProxySettings => settingsService.UserSettings.ProxySettings;
    private readonly HostCountryResolver? _hostCountryResolver =
        ipLocationProvider != null ? new HostCountryResolver(ipLocationProvider) : null;

    public AppProxyNodeInfo[] GetNodeInfos()
    {
        var data = Update();
        return data.NodeInfos;
    }

    private ServiceData Update()
    {
        var connectionInfo = vpnServiceManager.ConnectionInfo;
        var runtimeNodes = connectionInfo.SessionStatus?.ProxyManagerStatus.ProxyNodeInfos ?? [];

        // load last node states and sync it with user settings
        _data ??= JsonUtils.TryDeserializeFile<ServiceData>(_infoFilePath) ?? new ServiceData();
        _data.NodeInfos = SyncNodeInfosWithNodes(
            _data.NodeInfos, settingsService.UserSettings.ProxySettings.Nodes).ToArray();

        // update from runtimeNodes
        if (connectionInfo.CreatedTime > _data.UpdateTime && !vpnServiceManager.IsReconfiguring) {

            // overwrite Settings node if remote url list exists
            if (runtimeNodes.Any() &&
                ProxySettings.Mode is AppProxyMode.Custom &&
                ProxySettings.RemoteNotesUrl != null)
                ProxySettings.Nodes = runtimeNodes.Select(x => x.Node).ToArray();

            // update status
            var nodeDict = _data.NodeInfos.ToDictionary(info => info.Node.GetId(), info => info);
            foreach (var runtimeNode in runtimeNodes) {
                if (nodeDict.TryGetValue(runtimeNode.Node.GetId(), out var existing))
                    existing.Status = runtimeNode.Status;
            }

            _data.ResetStates = false;
            _data.UpdateTime = DateTime.Now;
        }

        return _data;
    }

    private static IEnumerable<AppProxyNodeInfo> SyncNodeInfosWithNodes(
        IEnumerable<AppProxyNodeInfo> nodeInfos, ProxyNode[] nodes)
    {
        // Pseudocode:
        // - Build a dictionary keyed by node id from existing infos.
        // - Iterate 'nodes' in the given order:
        //   - If an info exists, reuse it and update its Node reference to the current node.
        //   - If not, create a new AppProxyNodeInfo with default status and null CountryCode.
        // - Return the list in the same order as 'nodes'. Deleted nodes are naturally excluded.
        var nodeDict = nodeInfos.ToDictionary(info => info.Node.GetId(), info => info);

        var orderedInfos = new List<AppProxyNodeInfo>(nodes.Length);
        foreach (var node in nodes) {
            var id = node.GetId();
            if (nodeDict.TryGetValue(id, out var existing)) orderedInfos.Add(new AppProxyNodeInfo(node) {
                Status = existing.Status,
                CountryCode = null
            });
            else orderedInfos.Add(new AppProxyNodeInfo(node) {
                CountryCode = null,
            });
        }

        return orderedInfos;
    }

    // ReSharper disable once UnusedMember.Local
    private async Task UpdateCountryCode(IEnumerable<AppProxyNodeInfo> proxyNodeInfos, CancellationToken cancellationToken)
    {
        if (_hostCountryResolver is null)
            return;

        // resolve all host with domain name 
        proxyNodeInfos = proxyNodeInfos.Where(x => x.CountryCode is null).ToArray();
        var hostCountries = await _hostCountryResolver
            .GetHostCountries(proxyNodeInfos.Select(x => x.Node.Host), cancellationToken);

        foreach (var proxyNodeInfo in proxyNodeInfos)
            proxyNodeInfo.CountryCode = hostCountries.GetValueOrDefault(proxyNodeInfo.Node.Host);
    }


    public void ResetStates()
    {
        // remove from local state
        var data = Update();
        foreach (var nodeInfo in data.NodeInfos) 
            nodeInfo.Status = new ProxyNodeStatus();

        data.ResetStates = true;
        settingsService.Save();
    }

    private class ServiceData
    {
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        public AppProxyNodeInfo[] NodeInfos { get; set; } = [];
        public bool ResetStates { get; set; }
    }

    public ProxyOptions GetProxyOptions()
    {
        var data = Update();
        return new ProxyOptions {
            ResetStates = data.ResetStates,
            ProxyNodes = data.NodeInfos
                .Select(x => x.Node)
                .ToArray()
        };
    }
}