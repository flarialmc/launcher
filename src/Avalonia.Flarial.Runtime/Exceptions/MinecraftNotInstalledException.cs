using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftNotInstalledException : Win32Exception
{
    internal MinecraftNotInstalledException() : base((int)ERROR_INSTALL_PACKAGE_NOT_FOUND) { }
}