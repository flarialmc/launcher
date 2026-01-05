using System.Runtime.InteropServices;

namespace Flarial.Launcher;

[System.Security.SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    internal const int SW_NORMAL = 1;

    internal const uint SEM_NOGPFAULTERRORBOX = 0x0002;

    internal const uint SEM_FAILCRITICALERRORS = 0x0001;

    internal const uint SEM_NOOPENFILEERRORBOX = 0x8000;

    internal const uint SEM_NOALIGNMENTFAULTEXCEPT = 0x0004;

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern uint SetErrorMode(uint uMode);

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern nint ShellExecute(nint hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
}