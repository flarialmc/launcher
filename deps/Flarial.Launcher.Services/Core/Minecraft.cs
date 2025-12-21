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
using System;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract class Minecraft
{
    internal Minecraft() { }

    static readonly PackageManager s_packageManager = new();
    protected const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    protected static Package Package => s_packageManager.FindPackagesForUser(Empty, PackageFamilyName).First();

    public static Minecraft Current => UsingGameDevelopmentKit ? s_gdk : s_uwp;
    static readonly Minecraft s_uwp = new MinecraftUWP(), s_gdk = new MinecraftGDK();


    public bool IsRunning => Window is { };
    protected abstract string WindowClass { get; }

    protected abstract uint? Activate();
    public abstract uint? Launch(bool initialized);

    public static bool IsUnpackaged => Package.IsDevelopmentMode;
    public static bool IsInstalled => s_packageManager.FindPackagesForUser(Empty, PackageFamilyName).Any();

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

    private protected Win32Window? Window
    {
        get
        {
            fixed (char* @class = WindowClass)
            fixed (char* pfn = PackageFamilyName)
            {
                Win32Window window = HWND.Null;
                var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
                var buffer = stackalloc char[(int)length];

                while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    if (Win32Process.Open(PROCESS_QUERY_LIMITED_INFORMATION, window.ProcessId) is not { } process)
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