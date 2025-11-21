using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static System.StringComparison;
using System.IO;
using System.Reflection;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using Windows.Win32.Globalization;
using static Windows.ApplicationModel.Package;
using System.Threading.Tasks;
using System.Diagnostics;

static class GameLaunchHelper
{
    static readonly string s_path = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName;

    internal unsafe static bool HasPackageIdentity
    {
        get
        {
            var _ = 0U; if (GetCurrentPackageFamilyName(&_, null) != ERROR_INSUFFICIENT_BUFFER) return false;
            if (!Current.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", OrdinalIgnoreCase)) return false;
            return Path.Combine(Current.InstalledPath, "GameLaunchHelper.exe").Equals(s_path, OrdinalIgnoreCase);
        }
    }

    internal unsafe static bool Activate()
    {
        fixed (char* @class = "Bedrock") fixed (char* pfn1 = Current.Id.FamilyName)
        {
            HWND window = HWND.Null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var pfn2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                uint processId = 0; GetWindowThreadProcessId(window, &processId);
                var process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId); try
                {
                    if (GetPackageFamilyName(process, &length, pfn2) != ERROR_SUCCESS) continue;
                    if (CompareStringOrdinal(pfn1, -1, pfn2, -1, true) != COMPARESTRING_RESULT.CSTR_EQUAL) continue;
                    SwitchToThisWindow(window, true); return true;
                }
                finally { CloseHandle(process); }
            }
        }
        return false;
    }

    internal static async Task LaunchAsync() => await Task.Run(() =>
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = Current.InstalledPath,
            FileName = Path.Combine(Current.InstalledPath, "Minecraft.Windows.exe"),
        });
        process.WaitForInputIdle();
    });
}