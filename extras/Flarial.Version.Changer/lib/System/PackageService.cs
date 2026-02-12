using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Foundation.AsyncStatus;
using System.Threading;

namespace Flarial.Launcher.Services.System;

static class PackageService
{
    static readonly PackageManager s_packageManager = new();

    internal static Package? GetPackage(string packageFamilyName) => s_packageManager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    internal static void AddPackage(Uri uri, Action<int> action)
    {
        using ManualResetEventSlim @event = new();
        var info = s_packageManager.AddPackageAsync(uri, null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        try
        {
            info.Completed += (_, _) => @event.Set();
            info.Progress += (sender, args) => action((int)args.percentage);

            @event.Wait();
            if (info.Status is Error) throw info.ErrorCode;
        }
        finally { info.Close(); }
    }
}