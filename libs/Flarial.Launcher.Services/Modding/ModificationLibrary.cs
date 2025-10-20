using System.IO;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Modding;

public sealed class ModificationLibrary
{
    public readonly string FileName;

    public readonly bool IsValid;

    public readonly bool Exists;

    public ModificationLibrary(string path)
    {
        FileName = Path.GetFullPath(path);
        Exists = File.Exists(FileName) && Path.HasExtension(FileName);
        IsValid = Exists && FreeLibrary(LoadLibraryEx(FileName, LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES));
    }

    public static implicit operator ModificationLibrary(string @this) => new(@this);
}