using System.IO;
using System.Linq;
using Windows.Management.Deployment;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.Storage.FileSystem.FILE_SHARE_MODE;
using static Windows.Win32.Storage.FileSystem.FILE_ACCESS_RIGHTS;
using static Windows.Win32.Storage.FileSystem.FILE_CREATION_DISPOSITION;
using static Windows.Win32.PInvoke;
using Windows.Win32.Storage.FileSystem;
using Windows.ApplicationModel;
using System.Diagnostics;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract partial class Minecraft
{
    public static readonly Minecraft UWP = new MinecraftUWP();

    public static readonly Minecraft GDK = new MinecraftGDK();
}

partial class Minecraft
{
    protected static readonly string s_packageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    static readonly PackageManager s_packageManager = new();

    private protected static readonly IPackageDebugSettings s_packageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();

    static readonly IApplicationActivationManager s_applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();

    static Package Package
    {
        get
        {
            var packages = s_packageManager.FindPackagesForUser(string.Empty, s_packageFamilyName);
            return packages.FirstOrDefault() ?? throw new FileNotFoundException(null, s_packageFamilyName);
        }
    }
}

partial class Minecraft
{
    internal Minecraft() { }

    protected abstract string ApplicationUserModelId { get; }
}

unsafe partial class Minecraft
{
    private protected uint ActivateApplication()
    {
        fixed (char* appUserModelId = ApplicationUserModelId)
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

            GetPackagesByPackageFamily(s_packageFamilyName, ref count, &string1, ref length, string2);
            if (value) s_packageDebugSettings.EnableDebugging(string2, null, null);
            else s_packageDebugSettings.DisableDebugging(string2);
        }
    }

    public static bool IsInstalled
    {
        get
        {
            uint count = new(), length = new();
            var error = GetPackagesByPackageFamily(s_packageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    public static bool IsUnpackaged => Package.IsDevelopmentMode;
}

unsafe partial class Minecraft
{
    const uint DesiredAccess = (uint)FILE_READ_DATA;

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
            var version = Package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}

partial class Minecraft
{
    public abstract uint? Launch(bool initialized);

    public abstract void Terminate();

    public abstract bool IsRunning { get; }
}