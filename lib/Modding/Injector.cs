using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Flarial.Launcher.Services.Game;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;
using static Windows.Win32.Foundation.HANDLE;
using Windows.Win32.Foundation;
using static System.Security.AccessControl.FileSystemRights;
using static System.Security.AccessControl.AccessControlType;
using static System.Text.Encoding;
using Windows.Win32.System.Threading;

namespace Flarial.Launcher.Services.Modding;

using static System.NativeProcess;

public unsafe static class Injector
{
    static readonly LPTHREAD_START_ROUTINE s_routine;
    static readonly FileSystemAccessRule s_rule = new(new SecurityIdentifier("S-1-15-2-1"), FullControl, Allow);

    static Injector()
    {
        fixed (char* module = "Kernel32") fixed (byte* procedure = UTF8.GetBytes("LoadLibraryW"))
        {
            var address = GetProcAddress(GetModuleHandle(module), new(procedure));
            s_routine = address.CreateDelegate<LPTHREAD_START_ROUTINE>();
        }
    }

    public static uint? Launch(bool initialized, Library library)
    {
        if (!library.IsLoadable)
            throw new FileLoadException(null, library._path);

        var security = File.GetAccessControl(library._path);
        security.SetAccessRule(s_rule);
        File.SetAccessControl(library._path, security);

        if (Minecraft.Current.Launch(initialized) is not { } processId) return null;
        if (Open(PROCESS_ALL_ACCESS, processId) is not { } process) return null;

        using (process)
        {
            HANDLE thread = Null; void* address = null; try
            {
                var size = (nuint)(library._path.Length + 1) * sizeof(char);

                address = VirtualAllocEx(process, null, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                fixed (char* buffer = library._path) WriteProcessMemory(process, address, buffer, size, null);

                thread = CreateRemoteThread(process, null, 0, s_routine, address, 0, null);
                WaitForSingleObject(thread, INFINITE);

                return processId;
            }
            finally { CloseHandle(thread); VirtualFreeEx(process, address, 0, MEM_RELEASE); }
        }
    }
}