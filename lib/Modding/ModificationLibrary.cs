using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;

namespace Flarial.Launcher.Services.Modding;

public unsafe sealed class ModificationLibrary
{
    public readonly bool IsValid = false;
    public readonly string FileName = string.Empty;

    public ModificationLibrary(string path)
    {
        try
        {
            fixed (char* buffer = FileName = Path.GetFullPath(path))
            {
                if (!File.Exists(FileName) || !Path.HasExtension(FileName)) return;
                IsValid = FreeLibrary(LoadLibraryEx(buffer, Null, DONT_RESOLVE_DLL_REFERENCES));
            }
        }
        catch { }
    }

    public static implicit operator ModificationLibrary(string @this) => new(@this);
}