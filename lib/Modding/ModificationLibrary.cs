using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;

namespace Flarial.Launcher.Services.Modding;

/*
    - A caller should apply `SEM_FAILCRITICALERRORS` via `SetErrorMode()`.
    - This will prevent the `ModificationLibrary` class from blocking the caller.
*/

public unsafe sealed class ModificationLibrary
{
    public readonly bool IsValid;

    internal readonly string _path = string.Empty;

    public ModificationLibrary(string path)
    {
        /*
            - Ensure the provided path is correctly formatted.
            - The file should exist and have an extension.
        */

        try { _path = Path.GetFullPath(path); } catch { return; }
        if (!Path.HasExtension(_path) || !File.Exists(_path)) return;

        /*
            - Use `DONT_RESOLVE_DLL_REFERENCES` to load the library as stub.
            - This is done to perform load validation and to ensure no code is executed.
        */

        fixed (char* value = _path)
            IsValid = FreeLibrary(LoadLibraryEx(value, Null, DONT_RESOLVE_DLL_REFERENCES));
    }
}