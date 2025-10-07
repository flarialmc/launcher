using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Security.Authorization;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE;
using static Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS;
using static Windows.Win32.Security.OBJECT_SECURITY_INFORMATION;
using static Windows.Win32.Security.Authorization.SE_OBJECT_TYPE;
using static Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE;

namespace Flarial.Launcher.Services.Modding;

unsafe public sealed class Injector
{
    public static readonly Injector UWP = new(Minecraft.UWP);

    static readonly ACL* _acl;

    static Injector()
    {
        var module = GetModuleHandle("Kernel32");
        var procedure = GetProcAddress(module, "LoadLibraryW");
        _routine = Marshal.GetDelegateForFunctionPointer<LPTHREAD_START_ROUTINE>(procedure);

        fixed (char* name = "ALL APPLICATION PACKAGES")
        {
            TRUSTEE_W trustee = new()
            {
                ptstrName = name,
                TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_NAME,
                TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_WELL_KNOWN_GROUP
            };

            EXPLICIT_ACCESS_W access = new()
            {
                Trustee = trustee,
                grfAccessMode = ACCESS_MODE.SET_ACCESS,
                grfInheritance = ACE_FLAGS.SUB_CONTAINERS_AND_OBJECTS_INHERIT,
                grfAccessPermissions = (uint)GENERIC_ACCESS_RIGHTS.GENERIC_ALL
            };

            ACL* acl = null;
            SetEntriesInAcl(1, &access, null, &acl);

            _acl = acl;
        }
    }

    static readonly LPTHREAD_START_ROUTINE _routine;

    readonly Minecraft _minecraft;

    Injector(Minecraft minecraft) => _minecraft = minecraft;

    public uint? Launch(LaunchType type, ModificationLibrary library)
    {
        if (_minecraft.LaunchProcess(type) is not { } process)
            return null;

        using (process)
        {
            if (!process.IsRunning(0)) return null;
            if (!library.Exists) throw new FileNotFoundException(null, library.Filename);
            if (!library.Valid) throw new BadImageFormatException(null, library.Filename);

            fixed (char* filename = library.Filename)
            {
                SetNamedSecurityInfo(filename, SE_FILE_OBJECT, DACL_SECURITY_INFORMATION, PSID.Null, PSID.Null, _acl, null);

                void* address = null; HANDLE thread = HANDLE.INVALID_HANDLE_VALUE; try
                {
                    var size = (nuint)(library.Filename.Length + 1) * sizeof(char);
                    address = VirtualAllocEx(process, null, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                    WriteProcessMemory(process, address, filename, size, null);

                    thread = CreateRemoteThread(process, null, 0, _routine, address, 0, null);
                    WaitForSingleObject(thread, INFINITE);
                }
                finally
                {
                    VirtualFreeEx(process, address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
                    CloseHandle(thread);
                }
            }

            return process.ProcessId;
        }
    }
}