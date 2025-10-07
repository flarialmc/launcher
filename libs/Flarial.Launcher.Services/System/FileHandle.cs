using System;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Storage.FileSystem.FILE_INFO_BY_HANDLE_CLASS;
using static Windows.Win32.Storage.FileSystem.FILE_SHARE_MODE;
using static Windows.Win32.Storage.FileSystem.FILE_CREATION_DISPOSITION;
using static Windows.Win32.Foundation.HANDLE;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct FileHandle : IDisposable
{
    readonly HANDLE _fileHandle;

    internal static FileHandle? Open(PCWSTR path)
    {
        var fileHandle = CreateFile2(path, 0, FILE_SHARE_DELETE, OPEN_EXISTING, null);
        return fileHandle != INVALID_HANDLE_VALUE ? new(fileHandle) : null;
    }

    internal bool IsDeleted
    {
        get
        {
            FILE_STANDARD_INFO information = new();
            var size = (uint)sizeof(FILE_STANDARD_INFO);
            return GetFileInformationByHandleEx(_fileHandle, FileStandardInfo, &information, size) && information.DeletePending;
        }
    }

    FileHandle(HANDLE fileHandle) => _fileHandle = fileHandle;

    public void Dispose() => CloseHandle(_fileHandle);
}