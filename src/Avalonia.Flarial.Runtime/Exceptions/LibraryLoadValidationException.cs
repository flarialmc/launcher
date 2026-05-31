using System;
using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class LibraryLoadValidationException : Win32Exception
{
    internal LibraryLoadValidationException() : base((int)ERROR_DLL_INIT_FAILED) { }
}