using System.Linq;
using Windows.Management.Deployment;
using Windows.Win32.UI.Shell;
using static Windows.Win32.UI.Shell.ACTIVATEOPTIONS;
using System;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using Windows.Win32.Globalization;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Services.Core;

public abstract partial class Minecraft
{
    static readonly MinecraftUWP s_uwp = new();

    static readonly MinecraftGDK s_gdk = new();

    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : s_uwp;
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
    protected uint Activate()
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
            var version = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First().Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build / 100}";
        }
    }
}

unsafe partial class Minecraft
{
    protected uint? FindProcessId(string value)
    {
        fixed (char* name = value)
        fixed (char* id = ApplicationUserModelId)
        {
            uint level = 0, count = 0, length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            WTS_PROCESS_INFOW* information = null;
            var buffer = stackalloc char[(int)length];

            try
            {
                if (WTSEnumerateProcessesEx(WTS_CURRENT_SERVER_HANDLE, &level, WTS_CURRENT_SESSION, (PWSTR*)&information, &count))
                    for (var index = 0; index < count; index++)
                    {
                        var entry = information[index];

                        var result = CompareStringOrdinal(name, -1, entry.pProcessName, -1, true);
                        if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                        if (Win32Process.Open(PROCESS_QUERY_LIMITED_INFORMATION, entry.ProcessId) is not { } process)
                            continue;

                        using (process)
                        {

                            var error = GetApplicationUserModelId(process, &length, buffer);
                            if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                            result = CompareStringOrdinal(id, -1, buffer, -1, true);
                            if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                            return entry.ProcessId;
                        }
                    }

                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }

    private protected Win32Window? FindWindow(string value)
    {
        fixed (char* @class = value)
        fixed (char* id = ApplicationUserModelId)
        {
            Win32Window window = HWND.Null;
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var buffer = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                if (Win32Process.Open(PROCESS_QUERY_LIMITED_INFORMATION, window.ProcessId) is not { } process)
                    continue;

                using (process)
                {
                    var error = GetApplicationUserModelId(process, &length, buffer);
                    if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(id, -1, buffer, -1, true);
                    if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                    return window;
                }
            }

            return null;
        }
    }
}

partial class Minecraft
{
    public abstract uint? Launch(bool initialized);

    public abstract bool IsRunning { get; }
}