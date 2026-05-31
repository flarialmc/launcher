using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class DownloadUriNotFoundException : Win32Exception
{
    internal DownloadUriNotFoundException () : base((int)ERROR_APPX_RAW_DATA_WRITE_FAILED) { }
}