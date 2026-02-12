using static System.StringComparison;
using Windows.ApplicationModel;
using Flarial.Launcher.Services.System;

namespace Flarial.Launcher.Services.Game;

public abstract class Minecraft
{
    internal Minecraft() { }

    public static readonly string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    internal static Package Package => PackageService.GetPackage(PackageFamilyName)!;

    public static bool IsInstalled => Package is { };
    public static bool IsPackaged => Package.SignatureKind is PackageSignatureKind.Store;
    public static bool IsGamingServicesInstalled => PackageService.GetPackage("Microsoft.GamingServices_8wekyb3d8bbwe") is { };
    public static bool UsingGameDevelopmentKit => Package.GetAppListEntries()[0].AppUserModelId.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", OrdinalIgnoreCase);
}