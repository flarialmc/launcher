using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;
using System.IO;
using Windows.Win32.System.SystemServices;
using Windows.Win32.System.Diagnostics.Debug;
using static Windows.Win32.System.Diagnostics.Debug.IMAGE_FILE_CHARACTERISTICS;

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
            if (_path is null) return false;
            var module = HMODULE.Null;

            try
            {
                /*
                    - Use `DONT_RESOLVE_DLL_REFERENCES` to load the library as stub.
                    - This is done to perform load validation and to ensure no code is executed.
                */

                fixed (char* path = _path)
                {
                    module = LoadLibraryEx(path, dwFlags: DONT_RESOLVE_DLL_REFERENCES);
                    if (module.IsNull) return false;
                }

                /*
                    - Ensure the loaded library is actually a DLL.
                    - This can be done by inspecting the image header.
                */

                var dos = (IMAGE_DOS_HEADER*)(void*)module;
                var nt = (IMAGE_NT_HEADERS64*)((nint)dos + dos->e_lfanew);
                return nt->FileHeader.Characteristics.HasFlag(IMAGE_FILE_DLL);

            }
            finally { FreeLibrary(module); }
        }
    }

    public Library(string path)
    {
        _path = Path.GetFullPath(path);
        if (!Path.HasExtension(_path) || !File.Exists(_path)) _path = null!;
    }
}