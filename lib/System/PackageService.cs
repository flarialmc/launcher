using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Foundation.AsyncStatus;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe static class PackageService
{
    static readonly PackageManager s_packageManager = new();

    internal static Package? GetPackage(string packageFamilyName) => s_packageManager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    internal static void AddPackage(string path, Action<int> action)
    {
        var handle = CreateEvent(null, true, false, null);
        var info = s_packageManager.AddPackageAsync(new(path), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        try
        {
            info.Completed += (_, _) => SetEvent(handle);
            info.Progress += (sender, args) => action((int)args.percentage);

            WaitForSingleObject(handle, INFINITE);
            if (info.Status is Error) throw info.ErrorCode;
        }
        finally { CloseHandle(handle); info.Close(); }
    }
}