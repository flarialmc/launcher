using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class GamingServicesMissingException : Win32Exception
{
    internal GamingServicesMissingException() : base((int)ERROR_INSTALL_PREREQUISITE_FAILED) { }
}