using System.IO;
using System.Linq;
using Windows.Management.Deployment;
using Windows.Win32.UI.Shell;
using static Windows.Win32.UI.Shell.ACTIVATEOPTIONS;
using System.Diagnostics;
using System;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract partial class Minecraft
{
    public static readonly Minecraft UWP = new MinecraftUWP();

    public static readonly Minecraft GDK = new MinecraftGDK();
}

partial class Minecraft
{
    protected const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    protected static readonly PackageManager s_packageManager = new();

    static readonly IPackageDebugSettings s_packageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();

    static readonly IApplicationActivationManager s_applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();
}

partial class Minecraft
{
    internal Minecraft() { }

    protected abstract string ApplicationUserModelId { get; }
}

unsafe partial class Minecraft
{
    internal uint Activate()
    {
        fixed (char* applicationUserModelId = ApplicationUserModelId)
        {
            s_applicationActivationManager.ActivateApplication(applicationUserModelId, null, AO_NOERRORUI, out var processId);
            return processId;
        }
    }
}

unsafe partial class Minecraft
{
    public static bool IsInstalled => s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).Any();

    public static bool IsUnpackaged => s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First().IsDevelopmentMode;

    public static bool HasUWPAppLifecycle
    {
        set
        {
            var package = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();

            fixed (char* packageFullName = package.Id.FullName)
            {
                if (value) s_packageDebugSettings.EnableDebugging(packageFullName, null, null);
                else s_packageDebugSettings.DisableDebugging(packageFullName);
            }
        }
    }
}

unsafe partial class Minecraft
{
    public static bool UsingGameDevelopmentKit
    {
        get
        {
            var package = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();
            return package.GetAppListEntries()[0].AppUserModelId.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", StringComparison.OrdinalIgnoreCase);
        }
    }
}

partial class Minecraft
{
    public static string Version
    {
        get
        {
            var package = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();
            var path = Path.Combine(package.InstalledPath, "Minecraft.Windows.exe");

            var fileVersion = FileVersionInfo.GetVersionInfo(path).FileVersion;
            if (fileVersion is { }) return fileVersion.Substring(0, fileVersion.LastIndexOf('.'));

            var packageVersion = package.Id.Version;
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}";
        }
    }
}

partial class Minecraft
{
    public abstract uint? Launch(bool initialized);

    public abstract bool IsRunning { get; }
}