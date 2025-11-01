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

namespace Flarial.Launcher.Services.Modding;

public sealed class Injector
{
    public readonly static Injector UWP = new(Minecraft.UWP);

    public readonly static Injector GDK = new(Minecraft.GDK);

    internal readonly Minecraft _minecraft;

    static readonly FileSystemAccessRule s_rule = new(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, AccessControlType.Allow);

    static readonly LPTHREAD_START_ROUTINE s_routine = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryW").CreateDelegate<LPTHREAD_START_ROUTINE>();

    Injector(Minecraft minecraft) => _minecraft = minecraft;

    public uint? Launch(bool initialized, ModificationLibrary library)
    {
        if (!library.Exists) throw new FileNotFoundException(null, library.FileName);
        if (!library.IsValid) throw new BadImageFormatException(null, library.FileName);

        var security = File.GetAccessControl(library.FileName);
        security.SetAccessRule(s_rule);
        File.SetAccessControl(library.FileName, security);

        if (_minecraft.Launch(initialized) is not { } processId) return null;
        if (Win32Process.Open(PROCESS_ALL_ACCESS, processId) is not { } process) return null;
        using (process) using (new Win32RemoteThread(process, s_routine, library.FileName)) return processId;
    }
}