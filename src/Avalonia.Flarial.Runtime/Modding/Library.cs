using System.IO;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.System.SystemServices;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.Debug.IMAGE_FILE_CHARACTERISTICS;
using static Windows.Win32.System.LibraryLoader.LOAD_LIBRARY_FLAGS;

namespace Flarial.Runtime.Modding;

public unsafe sealed class Library
{
    internal string? FileName { get; }

    /*
        - A caller should apply `SEM_FAILCRITICALERRORS` via `SetErrorMode()`.
        - This will prevent `Library.IsLoadable` from blocking the caller.
    */

    public bool IsLoadable
    {
        get
        {
            if (FileName is null) return false;
            HMODULE module = new();

            try
            {
                /*
                    - Use `DONT_RESOLVE_DLL_REFERENCES` to load the library as stub.
                    - This is done to perform load validation and to ensure no code is executed.
                */

                fixed (char* path = FileName)
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
                return (nt->FileHeader.Characteristics & IMAGE_FILE_DLL) != 0;

            }
            finally { FreeLibrary(module); }
        }
    }

    public Library(string path)
    {
        try
        {
            FileName = Path.GetFullPath(path);
            if (!Path.HasExtension(FileName)) FileName = null;
        }
        catch { FileName = null; }
    }
}