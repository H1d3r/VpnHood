﻿using VpnHood.AppLib.Abstractions;
using VpnHood.AppLib.Services.Ads;
using VpnHood.Core.Client.Abstractions;
using VpnHood.Core.Client.VpnServices.Abstractions.Tracking;
using VpnHood.Core.Toolkit.Logging;
using VpnHood.Core.Toolkit.Utils;

namespace VpnHood.AppLib;

public class AppOptions(string appId, string storageFolderName, bool isDebugMode)
{
    public static string BuildStorageFolderPath(string subFolder) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subFolder);
    public string AppId => appId;
    public bool IsDebugMode => isDebugMode;
    public string StorageFolderPath { get; set; } = BuildStorageFolderPath(storageFolderName);
    public TimeSpan SessionTimeout { get; set; } = ClientOptions.Default.SessionTimeout;
    public TimeSpan VersionCheckInterval { get; set; } = TimeSpan.FromHours(24);
    public string? UpdateInfoUrl { get; set; }
    public bool UseInternalLocationService { get; set; } = true;
    public bool UseExternalLocationService { get; set; } = true;
    public AppResources Resources { get; set; } = new();
    // ReSharper disable once StringLiteralTypo
    public string? Ga4MeasurementId { get; set; } = "G-4LE99XKZYE";
    public string? UiName { get; set; }
    public bool IsAddAccessKeySupported { get; set; } = true;
    public bool IsLocalNetworkSupported { get; set; }
    public string[] AccessKeys { get; set; } = [];
    public IAppUiProvider? UiProvider { get; set; }
    public IAppCultureProvider? CultureProvider { get; set; }
    public IAppUpdaterProvider? UpdaterProvider { get; set; }
    public IAppAccountProvider? AccountProvider { get; set; }
    public IAppUserReviewProvider? UserReviewProvider { get; set; }
    public AppAdProviderItem[] AdProviderItems { get; set; } = [];
    public ITrackerFactory? TrackerFactory { get; set; }
    public TimeSpan UnstableTimeout { get; set; } = ClientOptions.Default.UnstableTimeout;
    public TimeSpan AutoWaitTimeout { get; set; } = ClientOptions.Default.AutoWaitTimeout;
    public bool? LogAnonymous { get; set; } =
        isDebugMode ? false : null; // it follows user's settings if it set to null
    public TimeSpan ServerQueryTimeout { get; set; } = ClientOptions.Default.ServerQueryTimeout;
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromMinutes(4).WhenNoDebugger();
    public TimeSpan TcpTimeout { get; set; } = ClientOptions.Default.ConnectTimeout;
    public TimeSpan LocationServiceTimeout { get; set; } = TimeSpan.FromSeconds(3); // ignore debugger attach to let check when there is no internet
    public bool AutoDiagnose { get; set; } = true;
    public AppAdOptions AdOptions { get; set; } = new();
    public bool AllowEndPointTracker { get; set; }
    public string? DeviceId { get; set; }
    public string? LocalSpaHostName { get; set; }
    public TimeSpan? EventWatcherInterval { get; set; } // set if you don't call State periodically
    public bool DisconnectOnDispose { get; set; }
    public LogServiceOptions LogServiceOptions { get; set; } = new();
    public bool AdjustForSystemBars { get; set; } = true;
    public bool AllowEndPointStrategy { get; set; }
    public object? CustomData { get; set; }
    public bool AutoRemoveExpiredPremium { get; set; } // auto remove premium on access code reject
    public AppFeature[] PremiumFeatures { get; init; } = [];
    public bool AllowRecommendUserReviewByServer { get; set; }
    public Uri? RemoteSettingsUrl { get; set; }
}