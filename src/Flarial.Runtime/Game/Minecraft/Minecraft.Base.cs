using System;
using System.IO;
using Flarial.Runtime.Services;
using Windows.ApplicationModel;

namespace Flarial.Runtime.Game;

public static partial class Minecraft
{
    const string ClassName = "Bedrock";
    const string ProcessName = "Minecraft.Windows.exe";
    const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    const string Command = $"Invoke-CommandInDesktopPackage -PackageFamilyName '{PackageFamilyName}' -AppId 'Game' -Command '{{0}}'";

    static Minecraft()
    {
        s_catalog.PackageUpdating += OnPackageUpdating;
        s_catalog.PackageInstalling += OnPackageInstalling;
        s_catalog.PackageUninstalling += OnPackageUninstalling;

        var system = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        s_path = Path.Combine(appdata, @"Minecraft Bedrock\Users");
        s_filename = Path.Combine(system, @"WindowsPowerShell\v1.0\powershell.exe");
    }

    internal static Package Package => PackageService.Get(PackageFamilyName)!;
    internal static string Version { get { var _ = Package.Id.Version; return $"{_.Major}.{_.Minor}.{_.Build / 100}"; } }

    public static bool IsInstalled => Package is { };
    public static bool IsPackaged => Package.SignatureKind is PackageSignatureKind.Store;
    public static bool IsGamingServicesInstalled => PackageService.Get("Microsoft.GamingServices_8wekyb3d8bbwe") is { };
}