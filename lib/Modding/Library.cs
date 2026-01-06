using Windows.Win32.Foundation;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;
using System.IO;
using Windows.Win32.System.SystemServices;
using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;
using static Windows.Win32.System.Diagnostics.Debug.IMAGE_FILE_CHARACTERISTICS;
using System;

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
if (_path is null)
return false;

            var module = HMODULE.Null;
            try
            {
                fixed (char* path = _path)
                {
                    /*
                        - Use `DONT_RESOLVE_DLL_REFERENCES` to load the library as stub.
                        - This is done to perform load validation and to ensure no code is executed.
                    */

                    module = LoadLibraryEx(path, dwFlags: DONT_RESOLVE_DLL_REFERENCES);

                    if (module.IsNull)
                        return false;
                }

                /*
                    - Ensure the loaded library is actually a DLL.
                    - This can be done by inspecting the image header.
                */

                var dos = (IMAGE_DOS_HEADER*)module;
                var nt = (IMAGE_NT_HEADERS64*)((nint)dos + dos->e_lfanew);

                return nt->FileHeader.Characteristics.HasFlag(IMAGE_FILE_DLL);

            }
            finally { FreeLibrary(module); }
        }
    }

    public Library(string path)
    {
        /*
            - Peform path validation.
            - Ensure the path has an extension & exists.
        */

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