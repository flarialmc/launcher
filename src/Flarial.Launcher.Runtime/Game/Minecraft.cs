using System;
using Flarial.Launcher.Runtime.Services;
using Flarial.Launcher.Runtime.System;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;
using static System.StringComparison;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Runtime.Game;

using static System.NativeProcess;

public unsafe abstract class Minecraft
{
    internal Minecraft() { }

    protected abstract string Class { get; }
    protected abstract string Executable { get; }

    public static readonly string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : throw new PlatformNotSupportedException();

    static readonly Minecraft s_gdk = new MinecraftGDK();
    internal static Package Package => PackageService.GetPackage(PackageFamilyName)!;

    internal static string Version
    {
        get
        {
            var version = Package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build / 100}";
        }
    }

    public static bool? AllowUnsignedInstalls
    {
        get => field ??= false;
        set => field ??= value;
    }

    protected abstract uint? Activate();
    public abstract uint? Launch(bool initialized);

    private protected NativeWindow? GetWindow() => GetWindow(Class);
    private protected uint? GetProcessId() => GetProcessId(Executable);

    public bool IsRunning => GetWindow() is { };
    public static bool IsInstalled => Package is { };
    public static bool IsPackaged => Package.SignatureKind is PackageSignatureKind.Store;
    public static bool IsGamingServicesInstalled => PackageService.GetPackage("Microsoft.GamingServices_8wekyb3d8bbwe") is { };
    public static bool UsingGameDevelopmentKit => Package.GetAppListEntries()[0].AppUserModelId.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", OrdinalIgnoreCase);

    internal static uint? GetProcessId(string target)
    {
        fixed (char* name = target)
        fixed (char* pfn = PackageFamilyName)
        {
            uint level = 0, count = 0, length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            WTS_PROCESS_INFOW* information = null;
            var buffer = stackalloc char[(int)length];

            try
            {
                if (WTSEnumerateProcessesEx(WTS_CURRENT_SERVER_HANDLE, &level, WTS_CURRENT_SESSION, (PWSTR*)&information, &count))
                    for (var index = 0; index < count; index++)
                    {
                        var entry = information[index];
                        if (CompareStringOrdinal(name, -1, entry.pProcessName, -1, true) != CSTR_EQUAL) continue;
                        if (Open(PROCESS_QUERY_LIMITED_INFORMATION, entry.ProcessId) is not { } process) continue;

                        using (process)
                        {
                            if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS) continue;
                            if (CompareStringOrdinal(pfn, -1, buffer, -1, true) != CSTR_EQUAL) continue;
                            return entry.ProcessId;
                        }
                    }
                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }

    internal static NativeWindow? GetWindow(string target)
    {
        fixed (char* name = target)
        fixed (char* pfn = PackageFamilyName)
        {
            NativeWindow window = HWND.Null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var buffer = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, name, null)) != HWND.Null)
            {
                if (Open(PROCESS_QUERY_LIMITED_INFORMATION, window.ProcessId) is not { } process)
                    continue;

                using (process)
                {
                    if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS) continue;
                    if (CompareStringOrdinal(pfn, -1, buffer, -1, true) != CSTR_EQUAL) continue;
                    return window;
                }
            }

            return null;
        }
    }
}