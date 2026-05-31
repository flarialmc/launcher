using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class InvalidLibraryPathException : Win32Exception
{
    internal InvalidLibraryPathException() : base((int)ERROR_BAD_PATHNAME) { }
}