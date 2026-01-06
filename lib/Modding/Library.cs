using Windows.Win32.Foundation;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;
using System.IO;

namespace Flarial.Launcher.Services.Modding;

public unsafe sealed class Library
{
    internal readonly string _path;

    /*
        - A caller should apply `SEM_FAILCRITICALERRORS` via `SetErrorMode()`.
        - This will prevent `Library.IsLoadable` from blocking the caller.
    */

    public bool IsLoadable
    {
        get
        {
            var library = HMODULE.Null;
            try
            {
                fixed (char* path = _path)
                {
                    /*
                        - Use `DONT_RESOLVE_DLL_REFERENCES` to load the library as stub.
                        - This is done to perform load validation and to ensure no code is executed.
                    */

                    library = LoadLibraryEx(path, dwFlags: DONT_RESOLVE_DLL_REFERENCES);
                    return library != HMODULE.Null;
                }
            }
            finally { FreeLibrary(library); }
        }
    }

    public Library(string path)
    {
        _path = Path.GetFullPath(path);

        if (!Path.HasExtension(_path))
        {
            _path = null!;
            return;
        }

        if (!File.Exists(_path))
        {
            _path = null!;
            return;
        }

    }
}