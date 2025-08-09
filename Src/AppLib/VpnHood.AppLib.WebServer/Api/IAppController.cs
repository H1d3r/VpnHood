﻿using VpnHood.AppLib.Settings;
using VpnHood.Core.Client.Device;
using VpnHood.Core.Common.Messaging;
using VpnHood.Core.Common.Tokens;

namespace VpnHood.AppLib.WebServer.Api;

public interface IAppController
{
    Task ProcessTypes(ExceptionType exceptionType, SessionErrorCode errorCode);
    Task<AppData> Configure(ConfigParams configParams);
    Task<AppData> GetConfig();
    Task<IpFilters> GetIpFilters();
    Task SetIpFilters(IpFilters ipFilters);
    Task<AppState> GetState();
    Task Connect(Guid? clientProfileId, string? serverLocation, ConnectPlanId planId);
    Task Diagnose(Guid? clientProfileId, string? serverLocation, ConnectPlanId planId);
    Task Disconnect();
    Task ClearLastError();
    Task SetUserSettings(UserSettings userSettings);
    Task<string> Log();
    Task<DeviceAppInfo[]> GetInstalledApps();
    Task VersionCheck();
    Task VersionCheckPostpone();
    Task RequestAlwaysOn();
    Task RequestQuickLaunch();
    Task RequestNotification();
    Task RequestUserReview();
    Task ExtendByRewardedAd();
    Task SetUserReview(int value);
    Task<CountryInfo[]> GetCountries();
}