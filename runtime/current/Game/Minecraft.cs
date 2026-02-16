using System;
using Flarial.Launcher.Runtime.Services;
using Flarial.Launcher.Runtime.System;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static System.StringComparison;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Runtime.Game;

using static System.NativeProcess;

public unsafe abstract class Minecraft
{
    internal Minecraft() { }
    protected abstract string Class { get; }

    public static readonly string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : throw new NotSupportedException();

    static readonly Minecraft s_gdk = new MinecraftGDK();
    internal static Package Package => PackageService.Get(PackageFamilyName)!;

    internal static string Version
    {
        get
        {
            var _ = Package.Id.Version;
            return $"{_.Major}.{_.Minor}.{_.Build / 100}";
        }
    }

    public static bool? AllowUnsignedInstalls
    {
        get => field ??= false;
        set => field ??= value;
    }

    protected abstract uint? Activate();
    public abstract uint? Launch(bool initialized);

    public bool IsRunning => GetWindow() is { };
    public static bool IsInstalled => Package is { };
    public static bool IsPackaged => Package.SignatureKind is PackageSignatureKind.Store;
    public static bool IsGamingServicesInstalled => PackageService.Get("Microsoft.GamingServices_8wekyb3d8bbwe") is { };
    public static bool UsingGameDevelopmentKit => Package.GetAppListEntries()[0].AppUserModelId.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", OrdinalIgnoreCase);

    private protected NativeWindow? GetWindow()
    {
        fixed (char* @class = Class)
        fixed (char* pfn = PackageFamilyName)
        {
            NativeWindow window = HWND.Null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var buffer = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
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