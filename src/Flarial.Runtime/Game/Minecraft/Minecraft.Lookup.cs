using Flarial.Runtime.Unmanaged;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe partial class Minecraft
{
    static uint? GetProcessId()
    {
        fixed (char* processNamePtr = ProcessName)
        fixed (char* packageFamilyNamePtr = PackageFamilyName)
        {
            uint level = 0, count = 0;
            uint length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;

            WTS_PROCESS_INFOW* processInfoPtr = null;
            var stringPtr = stackalloc char[(int)length];

            if (!WTSEnumerateProcessesEx(new(), &level, WTS_CURRENT_SESSION, (PWSTR*)&processInfoPtr, &count))
                return null;

            try
            {
                for (uint index = 0; index < count; index++)
                {
                    var processInfo = processInfoPtr[index];

                    if (CompareStringOrdinal(processNamePtr, -1, processInfo.pProcessName, -1, true) != CSTR_EQUAL)
                        continue;

                    if (NativeProcess.Open(PROCESS_QUERY_LIMITED_INFORMATION, processInfo.ProcessId) is not { } process)
                        continue;

                    using (process)
                    {
                        if (GetPackageFamilyName(process, &length, stringPtr) != ERROR_SUCCESS)
                            continue;

                        if (CompareStringOrdinal(packageFamilyNamePtr, -1, stringPtr, -1, true) != CSTR_EQUAL)
                            continue;

                        return processInfo.ProcessId;
                    }
                }
                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, processInfoPtr, count); }
        }
    }

    internal static NativeWindow? GetWindow(uint? processId = null, string className = ClassName)
    {
        fixed (char* classNamePtr = className)
        fixed (char* packageFamilyNamePtr = PackageFamilyName)
        {
            NativeWindow window = HWND.Null;
            var length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            var stringPtr = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, classNamePtr, null)) != HWND.Null)
            {
                if (processId is { } && processId != window._processId)
                    continue;

                if (NativeProcess.Open(PROCESS_QUERY_LIMITED_INFORMATION, window._processId) is not { } process)
                    continue;

                using (process)
                {
                    if (GetPackageFamilyName(process, &length, stringPtr) != ERROR_SUCCESS)
                        continue;

                    if (CompareStringOrdinal(packageFamilyNamePtr, -1, stringPtr, -1, true) != CSTR_EQUAL)
                        continue;

                    return window;
                }
            }

            return null;
        }
    }
}