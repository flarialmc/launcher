using System;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Storage.FileSystem.FILE_ACCESS_RIGHTS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Storage.FileSystem.FILE_SHARE_MODE;
using static Windows.Win32.Storage.FileSystem.FILE_CREATION_DISPOSITION;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32File : IDisposable
{
    readonly HANDLE _handle;

    internal static Win32File? TryOpen(PCWSTR path)
    {
        const FILE_ACCESS_RIGHTS access = FILE_READ_DATA;
        const FILE_SHARE_MODE mode = FILE_SHARE_DELETE | FILE_SHARE_READ | FILE_SHARE_WRITE;

        var handle = CreateFile2(path, (uint)access, mode, OPEN_EXISTING, null);
        return handle != HANDLE.INVALID_HANDLE_VALUE ? new(handle) : null;
    }

    internal Win32File(HANDLE handle) => _handle = handle;

    internal bool? IsDeleted
    {
        get
        {
            var size = (uint)sizeof(FILE_STANDARD_INFO);
            FILE_STANDARD_INFO information = new();
            const FILE_INFO_BY_HANDLE_CLASS @class = FILE_INFO_BY_HANDLE_CLASS.FileStandardInfo;
            return GetFileInformationByHandleEx(_handle, @class, &information, size) && information.DeletePending;
        }
    }

    public void Dispose() => CloseHandle(_handle);
}