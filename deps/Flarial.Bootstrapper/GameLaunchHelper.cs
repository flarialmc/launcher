using static NativeMethods;
using static System.StringComparison;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel;
using System.Runtime.CompilerServices;

static class GameLaunchHelper
{
    static readonly string s_path = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName;

    internal static bool HasPackageIdentity
    {
        get
        {
            try
            {
                var package = Package.Current;
                if (!package.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", OrdinalIgnoreCase)) return false;
                return Path.Combine(package.InstalledPath, "GameLaunchHelper.exe").Equals(s_path, OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }

    internal unsafe static bool Activate()
    {
        fixed (char* @class = "Bedrock") fixed (char* pfn1 = Package.Current.Id.FamilyName)
        {
            void* window = null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var pfn2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(null, window, @class, null)) != null)
            {
                uint processId = 0; GetWindowThreadProcessId(window, &processId);
                var process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId); try
                {
                    if (GetPackageFamilyName(process, &length, pfn2) != ERROR_SUCCESS) continue;
                    if (CompareStringOrdinal(pfn1, -1, pfn2, -1, true) != CSTR_EQUAL) continue;
                    SwitchToThisWindow(window, true); return true;
                }
                finally { CloseHandle(process); }
            }
        }
        return false;
    }

    internal static Request Launch() => new();

    internal sealed class Request
    {
        readonly Process _process;
        readonly TaskCompletionSource<bool> _source = new();

        internal Request()
        {
            var path = Package.Current.InstalledPath;

            _process = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = path,
                FileName = Path.Combine(path, "Minecraft.Windows.exe")
            });

            Task.Run(() =>
            {
                _process.WaitForInputIdle();
                _source.TrySetResult(new());
            });
        }

        internal TaskAwaiter GetAwaiter() => ((Task)_source.Task).GetAwaiter();

        internal void Cancel()
        {
            if (_source.Task.IsCompleted) return;
            if (!_process.HasExited) _process.Kill();
        }

        ~Request() => _process.Dispose();
    }
}