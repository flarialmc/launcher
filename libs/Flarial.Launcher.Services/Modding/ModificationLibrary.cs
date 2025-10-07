using static Windows.Win32.PInvoke;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;
using System.IO;

namespace Flarial.Launcher.Services.Modding;

public sealed class ModificationLibrary
{
    public readonly string Filename;

    public readonly bool Valid;

    public readonly bool Exists;

    internal ModificationLibrary(string path)
    {
        Filename = Path.GetFullPath(path);
        Exists = File.Exists(Filename);
        Valid = Exists && FreeLibrary(LoadLibraryEx(Filename, DONT_RESOLVE_DLL_REFERENCES));
    }

    public static implicit operator ModificationLibrary(string path) => new(path);
}