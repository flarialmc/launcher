using System.IO;
using System.Linq;
using Flarial.Launcher.Services.System;
using Windows.Management.Deployment;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.Storage.FileSystem.FILE_SHARE_MODE;
using static Windows.Win32.Foundation.GENERIC_ACCESS_RIGHTS;
using static Windows.Win32.Storage.FileSystem.FILE_CREATION_DISPOSITION;
using static Windows.Win32.PInvoke;
using Windows.Win32.Storage.FileSystem;
using Windows.ApplicationModel;
using System.Diagnostics;
using System;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract partial class Minecraft
{
    public static readonly Minecraft UWP = new MinecraftUWP();
}

partial class Minecraft
{
    protected const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    static readonly PackageManager s_packageManager = new();

    private protected static readonly IPackageDebugSettings s_packageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();

    static readonly IApplicationActivationManager s_applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();

    static Package Package
    {
        get
        {
            var packages = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName);
            return packages.FirstOrDefault() ?? throw new FileNotFoundException(null, PackageFamilyName);
        }
    }
}

partial class Minecraft
{
    protected readonly string _applicationUserModelId;

    internal Minecraft(string applicationUserModelId) => _applicationUserModelId = applicationUserModelId;
}

unsafe partial class Minecraft
{
    private protected uint ActivateApplication()
    {
        fixed (char* appUserModelId = _applicationUserModelId)
        {
            const ACTIVATEOPTIONS options = ACTIVATEOPTIONS.AO_NOERRORUI;
            s_applicationActivationManager.ActivateApplication(appUserModelId, null, options, out var processId);
            return processId;
        }
    }
}

unsafe partial class Minecraft
{
    public static bool HasUWPAppLifecycle
    {
        set
        {
            uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
            PWSTR string1 = new(), string2 = stackalloc char[(int)length];

            GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);
            if (value) s_packageDebugSettings.EnableDebugging(string2, null, null);
            else s_packageDebugSettings.DisableDebugging(string2);
        }
    }

    public static bool IsInstalled
    {
        get
        {
            uint count = new(), length = new();
            var error = GetPackagesByPackageFamily(PackageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    public static bool IsUnpackaged => Package.IsDevelopmentMode;
}

unsafe partial class Minecraft
{
    const uint DesiredAccess = (uint)GENERIC_READ;

    const FILE_SHARE_MODE ShareMode = FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE;

    public static bool UsingGameDevelopmentKit
    {
        get
        {
            fixed (char* path = Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe"))
            {
                var handle = CreateFile2(path, DesiredAccess, ShareMode, OPEN_EXISTING, null);
                try { return handle == HANDLE.INVALID_HANDLE_VALUE; }
                finally { CloseHandle(handle); }
            }
        }
    }
}

partial class Minecraft
{
    public static string ClientVersion
    {
        get
        {
            var path = Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe");
            var version = FileVersionInfo.GetVersionInfo(path).FileVersion;
            return (version ??= "0.0.0.0").Substring(0, version.LastIndexOf('.'));
        }
    }

    public static string PackageVersion
    {
        get
        {
            var packageVersion = Package.Id.Version;

            var major = packageVersion.Major;
            var minor = packageVersion.Minor;
            var build = packageVersion.Build;
            var revision = packageVersion.Revision;

            var version = $"{new Version(major, minor, build, revision)}";
            return version.Substring(0, version.LastIndexOf('.'));
        }
    }
}

partial class Minecraft
{
    public abstract uint? LaunchGame(bool initialized);

    public abstract void TerminateGame();

    public abstract bool IsRunning { get; }
}