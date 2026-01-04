using System.Runtime.InteropServices;
using System.Security;

namespace Flarial.Launcher;

[SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    const int SW_NORMAL = 1;

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint ShellExecute(nint hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

    internal static void ShellExecute(nint hWnd, string lpFile)
    {
        if (ShellExecute(hWnd, "Open", lpFile, null!, null!, SW_NORMAL) <= 32)
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
    }


    const uint SEM_NOGPFAULTERRORBOX = 0x0002;
    const uint SEM_NOOPENFILEERRORBOX = 0x8000;
    const uint SEM_FAILCRITICALERRORS = 0x0001;
    const uint SEM_NOALIGNMENTFAULTEXCEPT = 0x0004;

    [DllImport("Kernel32", SetLastError = true)]
    static extern uint SetErrorMode(uint uMode);

    internal static void SetErrorMode() => SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOALIGNMENTFAULTEXCEPT);
}