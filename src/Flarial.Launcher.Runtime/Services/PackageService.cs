using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Foundation.AsyncStatus;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.Services;

unsafe static class PackageService
{
    static readonly PackageManager s_pm = new();

    static readonly RegisterPackageOptions s_rpo = new()
    {
        DeveloperMode = true,
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };

    internal static Package? GetPackage(string packageFamilyName) => s_pm.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    internal static bool AddPackage(Uri uri, Action<int> callback)
    {
        var handle = CreateEvent(null, true, false, null);
        var info = s_pm.AddPackageAsync(uri, null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        try
        {
            info.Completed += (_, _) => SetEvent(handle);
            info.Progress += (sender, args) => callback((int)args.percentage);

            WaitForSingleObject(handle, INFINITE);
            if (info.Status is Error) throw info.ErrorCode;

            var result = info.GetResults();
            if (result.ExtendedErrorCode is { } @_) throw @_;

            return result.IsRegistered;
        }
        finally
        {
            CloseHandle(handle);
            info.Close();
        }
    }

    internal static bool RegisterPackage(Package package)
    {
        var name = package.Id.FullName;

        var options = ForceApplicationShutdown;
        if (package.IsDevelopmentMode) options |= DevelopmentMode;

        var handle = CreateEvent(null, true, false, null);
        var info = s_pm.RegisterPackageByFullNameAsync(name, null, options);

        try
        {
            info.Completed += (_, _) => SetEvent(handle);

            WaitForSingleObject(handle, INFINITE);
            if (info.Status is Error) throw info.ErrorCode;

            var result = info.GetResults();
            if (result.ExtendedErrorCode is { } @_) throw @_;

            return result.IsRegistered;
        }
        finally
        {
            CloseHandle(handle);
            info.Close();
        }
    }
}