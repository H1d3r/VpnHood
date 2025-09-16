﻿using System.Security.Principal;
using System.Windows;
using VpnHood.AppLib;
using VpnHood.AppLib.Services.Updaters;
using VpnHood.AppLib.Win.Common;
using VpnHood.AppLib.Win.Common.WpfSpa;

namespace VpnHood.App.Client.Win.Web;

public class App : Application
{
    private static AppOptions CreateAppOptions(AppConfigs appConfigs)
    {
        var resources = ClientAppResources.Resources;
        resources.Strings.AppName = AppConfigs.AppName;

        return new AppOptions(appConfigs.AppId, appConfigs.StorageFolderName, AppConfigs.IsDebugMode) {
            CustomData = appConfigs.CustomData,
            DeviceId = WindowsIdentity.GetCurrent().User?.Value,
            Resources = resources,
            AccessKeys = AppConfigs.IsDebug ? [appConfigs.DefaultAccessKey] : [],
            IsAddAccessKeySupported = true,
            IsLocalNetworkSupported = true,
            LocalSpaHostName = "my-vpnhood",
            AllowRecommendUserReviewByServer = false,
            LogServiceOptions = {
                SingleLineConsole = false
            },
            UpdaterOptions = new AppUpdaterOptions {
                UpdateInfoUrl = appConfigs.UpdateInfoUrl,
                UpdaterProvider = new AdvancedInstallerUpdaterProvider(),
                PromptDelay = TimeSpan.FromDays(1)
            }
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // call base first to init app resources
        base.OnStartup(e);

        // load app configs
        var appConfigs = AppConfigs.Load();
        VpnHoodAppWpfSpa.Init(() => CreateAppOptions(appConfigs),
            spaListenToAllIps: appConfigs.SpaListenToAllIps,
            spaDefaultPort: appConfigs.SpaDefaultPort,
            args: Environment.GetCommandLineArgs());
    }

    [STAThread]
    public static void Main(string[] args)
    {
        var app = new App();
        app.Run();
    }


}