using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftUnpackagedException : Win32Exception
{
    internal MinecraftUnpackagedException() : base((int)ERROR_UNSIGNED_PACKAGE_INVALID_CONTENT) { }
}