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

    static readonly FileSystemAccessRule _rule = new(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, AccessControlType.Allow);

    static readonly LPTHREAD_START_ROUTINE _routine = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryW").CreateDelegate<LPTHREAD_START_ROUTINE>();

    Injector(Minecraft minecraft) => _minecraft = minecraft;

    public uint? Launch(bool initialized, ModificationLibrary library)
    {
        var parameter = library.FileName;

        if (!library.Exists) throw new FileNotFoundException(null, parameter);
        if (!library.IsValid) throw new BadImageFormatException(null, parameter);

        var security = File.GetAccessControl(parameter);
        security.SetAccessRule(_rule);
        File.SetAccessControl(parameter, security);

        if (_minecraft.Launch(initialized) is not { } processId)
            return null;

        if (Win32Process.Open(PROCESS_ALL_ACCESS, processId) is not { } process)
            return null;

        using (process)
        {
            using Win32RemoteThread thread = new(process, _routine, parameter);
            return processId;
        }
    }
}