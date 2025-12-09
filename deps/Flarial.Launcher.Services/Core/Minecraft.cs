using System.Linq;
using Windows.Management.Deployment;
using Windows.Win32.UI.Shell;
using static Windows.Win32.UI.Shell.ACTIVATEOPTIONS;
using static System.String;
using static System.StringComparison;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using Windows.Win32.Globalization;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using Windows.ApplicationModel;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract partial class Minecraft
{
    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : s_uwp;
    static readonly Minecraft s_uwp = new MinecraftUWP(), s_gdk = new MinecraftGDK();

    static readonly PackageManager s_packageManager = new();
    static readonly IPackageDebugSettings s_packageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();
    static readonly IApplicationActivationManager s_applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();

    internal Minecraft() { }
    public bool IsRunning => Window is { };
    protected abstract string WindowClass { get; }
    protected abstract string ApplicationUserModelId { get; }
    protected const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    public abstract uint? Launch(bool initialized);

    protected uint Activate()
    {
        fixed (char* id = ApplicationUserModelId)
        {
            s_applicationActivationManager.ActivateApplication(id, null, AO_NOERRORUI, out var processId);
            return processId;
        }
    }

    static Package Package => s_packageManager.FindPackagesForUser(Empty, PackageFamilyName).Single();

    public static bool IsInstalled => s_packageManager.FindPackagesForUser(Empty, PackageFamilyName).Any();

    public static bool IsUnpackaged => Package.IsDevelopmentMode;

    public static bool HasUWPAppLifecycle
    {
        set
        {
            fixed (char* packageFullName = Package.Id.FullName)
                if (value) s_packageDebugSettings.DisableDebugging(packageFullName);
                else s_packageDebugSettings.EnableDebugging(packageFullName, null, null);
        }
    }

    public static bool UsingGameDevelopmentKit
    {
        get
        {
            var appUserModelId = Package.GetAppListEntries()[0].AppUserModelId;
            return appUserModelId.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", OrdinalIgnoreCase);
        }
    }

    public static string Version
    {
        get
        {
            var version = Package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build / 100}";
        }
    }

    protected uint? GetProcessId(string value)
    {
        fixed (char* name = value) fixed (char* aumid = ApplicationUserModelId)
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

                            result = CompareStringOrdinal(aumid, -1, buffer, -1, true);
                            if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                            return entry.ProcessId;
                        }
                    }

                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }

    internal Win32Window? Window
    {
        get
        {
            fixed (char* @class = WindowClass) fixed (char* aumid = ApplicationUserModelId)
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

                        var result = CompareStringOrdinal(aumid, -1, buffer, -1, true);
                        if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                        return window;
                    }
                }

                return null;
            }
        }
    }
}