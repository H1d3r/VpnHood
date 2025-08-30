﻿using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Microsoft.Extensions.Logging;
using VpnHood.Core.Toolkit.ApiClients;
using VpnHood.Core.Toolkit.Logging;

namespace VpnHood.Core.Client.Device.Droid.Utils;

public static class AndroidUtil
{
    public static string GetAppName(Context? context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.PackageName);
        ArgumentNullException.ThrowIfNull(context.PackageManager);

        return context.PackageManager.GetApplicationLabel(
            context.PackageManager.GetApplicationInfo(context.PackageName, PackageInfoFlags.MetaData));
    }

    public static Task RunOnUiThread(Activity activity, Action action)
    {
        var taskCompletionSource = new TaskCompletionSource();
        activity.RunOnUiThread(() => {
            try {
                action();
                taskCompletionSource.TrySetResult();
            }
            catch (Exception ex) {
                taskCompletionSource.TrySetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static Task<T> RunOnUiThread<T>(Activity activity, Func<T> action)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        activity.RunOnUiThread(() => {
            try {
                var result = action();
                taskCompletionSource.TrySetResult(result);
            }
            catch (Exception ex) {
                taskCompletionSource.TrySetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static string? GetDeviceId(Context context)
    {
        try {
            return Android.Provider.Settings.Secure.GetString(
                context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }
        catch (Exception ex) {
            VhLogger.Instance.LogError(ex, "Could not retrieve android id.");
            return null;
        }
    }

    public static void ShowToast(string message)
    {
        var handler = new Handler(Looper.MainLooper!);
        handler.Post(() => {
            try {
                Toast.MakeText(Application.Context, message, ToastLength.Short)?.Show();
            }
            catch (Exception ex) {
                VhLogger.Instance.LogError(ex, "Error showing a toast");
            }
        });
    }

    public static ApiException JavaExceptionToApiException(Java.Lang.Object error)
    {
        return new ApiException(
            new ApiError {
                TypeFullName = error.Class.TypeName,
                TypeName = error.Class.SimpleName,
                Message = error.ToString()
            });
    }

    public static bool IsTv(Context? context = null)
    {
        var ctx = context ?? Application.Context;

        var uiMode = (UiModeManager?)ctx.GetSystemService(Context.UiModeService);
        if (uiMode?.CurrentModeType is UiMode.TypeTelevision)
            return true;

        var packageManager = ctx.PackageManager;
        return packageManager?.HasSystemFeature(PackageManager.FeatureLeanback) is true;
    }

}