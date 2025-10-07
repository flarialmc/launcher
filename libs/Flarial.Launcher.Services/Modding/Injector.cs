using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.System;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Modding;

public sealed class Injector
{
    public readonly static Injector UWP = new(Minecraft.UWP);

    readonly Minecraft _minecraft;

    static readonly FileSystemAccessRule _rule = new(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, AccessControlType.Allow);

    static readonly LPTHREAD_START_ROUTINE _routine = GetProcAddress(GetModuleHandle("Kernel32"), "LoadLibraryW").CreateDelegate<LPTHREAD_START_ROUTINE>();

    Injector(Minecraft minecraft) => _minecraft = minecraft;

    public uint? Launch(bool wait, DynamicLinkLibrary library)
    {
        var path = library.Path;

        if (!library.Exists) throw new FileNotFoundException(null, path);
        if (!library.Valid) throw new BadImageFormatException(null, path);

        var security = File.GetAccessControl(path);
        security.SetAccessRule(_rule);
        File.SetAccessControl(path, security);

        using var process = _minecraft.Activate(wait);
        if (!process.Running(0)) return null;

        Console.WriteLine(process.Running(0));

        using Win32RemoteThread thread = new(process, _routine, library.Path);
        return process.Id;
    }
}