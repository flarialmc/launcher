using System.Linq;
using Windows.Management.Deployment;
using static System.StringComparison;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using Windows.Win32.Globalization;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using Windows.ApplicationModel;
using Flarial.Launcher.Services.System;
using System;
using Windows.Win32.UI.Shell;

namespace Flarial.Launcher.Services.Game;

using static System.NativeProcess;

public unsafe abstract class Minecraft
{
    internal Minecraft() { }

    internal static readonly PackageManager s_packageManager = new();
    static readonly Minecraft s_uwp = new MinecraftUWP(), s_gdk = new MinecraftGDK();
    public static readonly string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    public bool Running => Window is { };
    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : s_uwp;
    internal static Package Package => s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();

    protected abstract string Class { get; }

    protected abstract uint? Activate();
    public abstract uint? Launch(bool initialized);

    [Obsolete("", true)]
    public static bool AllowUnsignedInstalls { get; set; }

    public static bool Packaged => Package.SignatureKind is PackageSignatureKind.Store;
    public static bool Installed => s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).Any();

    public static bool UsingGameDevelopmentKit
    {
        get
        {
            var aumid = Package.GetAppListEntries()[0].AppUserModelId;
            return aumid.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game", OrdinalIgnoreCase);
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

    private protected NativeWindow? Window
    {
        get
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
                        var error = GetPackageFamilyName(process, &length, buffer);
                        if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                        var result = CompareStringOrdinal(pfn, -1, buffer, -1, true);
                        if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                        return window;
                    }
                }

                return null;
            }
        }
    }
}