using System;
using System.ComponentModel;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Exceptions;

sealed class MinecraftNotFoundException : Win32Exception
{
    internal MinecraftNotFoundException() : base((int)ERROR_NEEDS_REMEDIATION) { }
}