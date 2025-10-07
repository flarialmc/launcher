using System;
using System.IO;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32File : IDisposable
{
    readonly HANDLE _handle;

    internal static Win32File? Open(PCWSTR path)
    {
        const FILE_SHARE_MODE mode = FILE_SHARE_MODE.FILE_SHARE_DELETE;
        const FILE_CREATION_DISPOSITION disposition = FILE_CREATION_DISPOSITION.OPEN_EXISTING;

        var handle = CreateFile2(path, 0, mode, disposition, null);
        return handle != HANDLE.INVALID_HANDLE_VALUE ? new(handle) : null;
    }

    internal Win32File(HANDLE handle) => _handle = handle;

    internal bool? Deleted
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