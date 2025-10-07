using System.IO;
using Windows.Win32.System.LibraryLoader;
using static System.IO.Path;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Modding;

public sealed class DynamicLinkLibrary
{
    public readonly string Path;

    public readonly bool Valid;

    public readonly bool Exists;

    internal DynamicLinkLibrary(string path)
    {
        const LOAD_LIBRARY_FLAGS flags = LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES;
        Path = GetFullPath(path); Exists = File.Exists(Path) && HasExtension(Path);
        Valid = Exists && FreeLibrary(LoadLibraryEx(Path, flags));
    }

    public static implicit operator DynamicLinkLibrary(string @this) => new(@this);
}