using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using static Windows.Foundation.AsyncStatus;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;

namespace Flarial.Runtime.Services;

public static class PackageRegistry
{
    static readonly PackageManager s_manager = new();
    static readonly PackageCatalog s_catalog = PackageCatalog.OpenForCurrentUser();

    internal static Package? Get(string packageFamilyName) => s_manager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();

    public static void WatchPackageChanges(string packageFamilyName, Action callback)
    {
        s_catalog.PackageInstalling += (_, args) =>
        {
            if (args.IsComplete && IsPackage(args.Package.Id.FamilyName, packageFamilyName))
                callback();
        };

        s_catalog.PackageUninstalling += (_, args) =>
        {
            if (args.IsComplete && IsPackage(args.Package.Id.FamilyName, packageFamilyName))
                callback();
        };

        s_catalog.PackageUpdating += (_, args) =>
        {
            if (args.IsComplete && IsPackage(args.TargetPackage.Id.FamilyName, packageFamilyName))
                callback();
        };
    }

    static bool IsPackage(string candidate, string packageFamilyName) =>
        candidate.Equals(packageFamilyName, StringComparison.OrdinalIgnoreCase);

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
