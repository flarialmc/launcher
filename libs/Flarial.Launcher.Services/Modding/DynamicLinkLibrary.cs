using System.IO;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Modding;

public sealed class DynamicLinkLibrary
{
    public readonly string Name;

    public readonly bool Valid;

    public readonly bool Exists;

    internal DynamicLinkLibrary(string name)
    {
        Name = Path.GetFullPath(name);
        Exists = File.Exists(Name) && Path.HasExtension(Name);
        Valid = Exists && FreeLibrary(LoadLibraryEx(Name, LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES));
    }

    public static implicit operator DynamicLinkLibrary(string @this) => new(@this);
}