using System;
using Windows.ApplicationModel;
using static System.StringComparison;

namespace Flarial.Runtime.Game;

partial class Minecraft
{
    static readonly PackageCatalog s_catalog = PackageCatalog.OpenForCurrentUser();

    static void OnPackageUpdating(PackageCatalog sender, PackageUpdatingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.TargetPackage);
    }

    static void OnPackageUninstalling(PackageCatalog sender, PackageUninstallingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.Package);
    }

    static void OnPackageInstalling(PackageCatalog sender, PackageInstallingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.Package);
    }

    static void OnPackageStatusChanged(Package package)
    {
        if (PackageFamilyName.Equals(package.Id.FamilyName, OrdinalIgnoreCase))
            PackageStatusChanged?.Invoke();
    }

    public static event Action? PackageStatusChanged;
}