using Flarial.Runtime.Unmanaged;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

public static class Injector
{
    static unsafe readonly delegate* unmanaged[Stdcall]<void*, uint> s_address;

    unsafe static Injector()
    {
        fixed (char* moduleNamePtr = "Kernel32")
        fixed (byte* procedureNamePtr = "LoadLibraryW"u8)
        {
            var module = GetModuleHandle(moduleNamePtr);
            var address = GetProcAddress(module, new(procedureNamePtr));
            s_address = (delegate* unmanaged[Stdcall]<void*, uint>)(nint)address;
        }
    }

    public unsafe static bool Launch( Library library)
    {
        var path = library.EnsureLoadable();

        if (Minecraft.Launch() is not { } processId)
            return false;

        if (NativeProcess.Open(PROCESS_ALL_ACCESS, processId) is not { } process)
            return false;

        using (process)
        {
            HANDLE thread = new();
            void* parameter = null;
            try
            {
                var size = (nuint)(path.Length + 1) * sizeof(char);

                parameter = VirtualAllocEx(process, null, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                fixed (char* buffer = path) WriteProcessMemory(process, parameter, buffer, size, null);

                thread = CreateRemoteThread(process, null, 0, s_address, parameter, 0, null);
                WaitForSingleObject(thread, INFINITE);

                return true;
            }
            finally
            {
                CloseHandle(thread);
                VirtualFreeEx(process, parameter, 0, MEM_RELEASE);
            }
        }
    }
}