using System.Runtime.InteropServices;
using Flarial.Runtime.Unmanaged;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe partial class Minecraft
{
    static uint? GetProcessId() => GetProcessId(ProcessName);

    internal static NativeWindow? GetWindow([Optional] uint? processId) => GetWindow(ClassName, processId);

    static uint? GetProcessId(string processName)
    {
        fixed (char* processNamePtr = processName)
        fixed (char* packageFamilyNamePtr = PackageFamilyName)
        {
            uint level = 0;
            uint count = 0;
            uint length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;

            WTS_PROCESS_INFOW* information = null;
            var buffer = stackalloc char[(int)length];

            try
            {
                if (WTSEnumerateProcessesEx(WTS_CURRENT_SERVER_HANDLE, &level, WTS_CURRENT_SESSION, (PWSTR*)&information, &count))
                    for (var index = 0; index < count; index++)
                    {
                        var entry = information[index];
                        if (CompareStringOrdinal(processNamePtr, -1, entry.pProcessName, -1, true) != CSTR_EQUAL) continue;
                        if (NativeProcess.Open(PROCESS_QUERY_LIMITED_INFORMATION, entry.ProcessId) is not { } process) continue;

                        using (process)
                        {
                            if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS) continue;
                            if (CompareStringOrdinal(packageFamilyNamePtr, -1, buffer, -1, true) != CSTR_EQUAL) continue;
                            return entry.ProcessId;
                        }
                    }
                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }

    internal static NativeWindow? GetWindow(string className, [Optional] uint? processId)
    {
        fixed (char* classNamePtr = className)
        fixed (char* packageFamilyNamePtr = PackageFamilyName)
        {
            NativeWindow window = HWND.Null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var buffer = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, classNamePtr, null)) != HWND.Null)
            {
                if (processId is { } && processId != window._processId)
                    continue;

                if (NativeProcess.Open(PROCESS_QUERY_LIMITED_INFORMATION, window._processId) is not { } process)
                    continue;

                using (process)
                {
                    if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS) continue;
                    if (CompareStringOrdinal(packageFamilyNamePtr, -1, buffer, -1, true) != CSTR_EQUAL) continue;
                    return window;
                }
            }

            return null;
        }
    }
}