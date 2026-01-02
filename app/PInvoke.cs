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
}