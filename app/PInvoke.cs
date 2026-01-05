using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Flarial.Launcher;

[SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    internal const int SW_NORMAL = 1;

    internal const uint SEM_FAILCRITICALERRORS = 0x0001;

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern uint SetErrorMode(uint uMode);

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern nint ShellExecute(nint hWnd, [Optional] string lpOperation, string lpFile, [Optional] string lpParameters, [Optional] string lpDirectory, int nShowCmd);
}