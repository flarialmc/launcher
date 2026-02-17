using System.IO;
using Flarial.Launcher.Runtime.Game;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static System.Text.Encoding;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Runtime.Modding;

using static System.NativeProcess;

public unsafe static class Injector
{
    static readonly delegate* unmanaged[Stdcall]<void*, uint> s_routine;

    static Injector()
    {
        fixed (char* module = "Kernel32")
        fixed (byte* procedure = UTF8.GetBytes("LoadLibraryW"))
        {
            var address = GetProcAddress(GetModuleHandle(module), new(procedure));
            s_routine = (delegate* unmanaged[Stdcall]<void*, uint>)address.Value;
        }
    }

    public static uint? Launch(bool initialized, Library library)
    {
        if (!library.IsLoadable) throw new FileLoadException(null, library._path);
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