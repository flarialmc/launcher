using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.System;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;
using static Windows.Win32.Foundation.HANDLE;
using Windows.Win32.Foundation;

namespace Flarial.Launcher.Services.Modding;

public static class Injector
{
    static readonly FileSystemAccessRule s_rule = new(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, AccessControlType.Allow);

    static readonly LPTHREAD_START_ROUTINE s_procedure = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryW").CreateDelegate<LPTHREAD_START_ROUTINE>();

    public static unsafe uint? Launch(bool initialized, ModificationLibrary library)
    {
        if (!library.Exists) throw new FileNotFoundException(null, library.FileName);
        if (!library.IsValid) throw new BadImageFormatException(null, library.FileName);

        var security = File.GetAccessControl(library.FileName);
        security.SetAccessRule(s_rule);
        File.SetAccessControl(library.FileName, security);

        if (Minecraft.Current.Launch(initialized) is not { } processId) return null;
        if (Win32Process.Open(PROCESS_ALL_ACCESS, processId) is not { } process) return null;

        using (process)
        {
            HANDLE thread = Null; void* address = null; try
            {
                var size = (nuint)(library.FileName.Length + 1) * sizeof(char);

                address = VirtualAllocEx(process, null, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                fixed (char* buffer = library.FileName) WriteProcessMemory(process, address, buffer, size, null);

                thread = CreateRemoteThread(process, null, 0, s_procedure, address, 0, null);
                WaitForSingleObject(thread, INFINITE);

                return processId;
            }
            finally { CloseHandle(thread); VirtualFreeEx(process, address, 0, MEM_RELEASE); }
        }
    }
}