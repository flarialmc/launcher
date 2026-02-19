using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Foundation.AsyncStatus;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.Services;

unsafe static class PackageService
{
    static readonly PackageManager s_packageManager = new();

    internal static Package? Get(string packageFamilyName) => s_packageManager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    internal static void Add(Uri uri, Action<int> callback)
    {
        var handle = CreateEvent(null, true, false, null);
        var info = s_packageManager.AddPackageAsync(uri, null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        try
        {
            info.Completed += (_, _) => SetEvent(handle);
            info.Progress += (sender, args) => callback((int)args.percentage);

            WaitForSingleObject(handle, INFINITE);
            if (info.Status is Error) throw info.ErrorCode;
        }
        finally { CloseHandle(handle); info.Close(); }
    }
}