using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Foundation.AsyncStatus;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;

namespace Flarial.Runtime.Services;

static class PackageRegistry
{
    static readonly PackageManager s_manager = new();

    internal static Package? Get(string packageFamilyName) => s_manager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    unsafe static void Add(Uri uri, Action<int> callback)
    {
        var handle = CreateEvent(null, true, false, null);
        var info = s_manager.AddPackageAsync(uri, null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        try
        {
            info.Completed += (_, _) => SetEvent(handle);
            info.Progress += (sender, args) => callback((int)args.percentage);

            WaitForSingleObject(handle, INFINITE);
            if (info.Status is Error) throw info.ErrorCode;
        }
        finally
        {
            CloseHandle(handle);
            info.Close();
        }
    }

    internal static async Task AddAsync(Uri uri, Action<int> callback) => await Task.Run(() => Add(uri, callback));
}