using System.Text;
using Flarial.Runtime.Unmanaged;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

public static class Injector
{
    static unsafe readonly LPTHREAD_START_ROUTINE s_address;

    unsafe static Injector()
    {
        fixed (char* module = "Kernel32")
        fixed (byte* procedure = Encoding.UTF8.GetBytes("LoadLibraryW"))
        {
            var address = GetProcAddress(GetModuleHandle(module), new(procedure));
            s_address = address.CreateDelegate<LPTHREAD_START_ROUTINE>();
        }
    }

    public unsafe static uint? Launch(Library library)
    {
        var path = library.EnsurePath();

        if (Minecraft.Launch() is not { } processId)
            return null;

        if (NativeProcess.Open(PROCESS_ALL_ACCESS, processId) is not { } process)
            return null;

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

                return processId;
            }
            finally
            {
                CloseHandle(thread);
                VirtualFreeEx(process, parameter, 0, MEM_RELEASE);
            }
        }
    }
}