using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;

namespace Flarial.Launcher.Services.Modding;

/*
    - The caller should apply `SEM_FAILCRITICALERRORS` to prevent hangs.
    - To perform validation, the library is loaded as a stub.
    - This class is meant to be shorted lived & not cached.
*/

public unsafe sealed class ModificationLibrary
{
    public readonly bool IsValid;

    internal readonly string _path = string.Empty;

    public ModificationLibrary(string path)
    {
        try
        {
            fixed (char* buffer = _path = Path.GetFullPath(path))
            {
                if (!File.Exists(_path) || !Path.HasExtension(_path)) return;

                /*
                    - Use `DONT_RESOLVE_DLL_REFERENCES` to the library as stub.
                    - This is done to perform load validation and to ensure no code is executed.
                */

                IsValid = FreeLibrary(LoadLibraryEx(buffer, Null, DONT_RESOLVE_DLL_REFERENCES));
            }
        }
        catch { }
    }

    public static implicit operator ModificationLibrary(string @this) => new(@this);
}