using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;

namespace Flarial.Launcher.Services.Modding;

public unsafe sealed class ModificationLibrary
{
    public readonly string FileName;

    public readonly bool IsValid, Exists;

    public ModificationLibrary(string path)
    {
        FileName = Path.GetFullPath(path);
        Exists = File.Exists(FileName) && Path.HasExtension(FileName);
        fixed (char* name = FileName) IsValid = Exists && FreeLibrary(LoadLibraryEx(name, Null, DONT_RESOLVE_DLL_REFERENCES));
    }

    public static implicit operator ModificationLibrary(string @this) => new(@this);
}