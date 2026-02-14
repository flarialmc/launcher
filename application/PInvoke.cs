using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Flarial.Launcher;

[System.Security.SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    const int SW_NORMAL = 1;
    const uint SEM_NOGPFAULTERRORBOX = 0x0002;
    const uint SEM_FAILCRITICALERRORS = 0x0001;
    const uint SEM_NOOPENFILEERRORBOX = 0x8000;
    const uint SEM_NOALIGNMENTFAULTEXCEPT = 0x0004;

    [DllImport("Kernel32", SetLastError = true)]
    static extern uint SetErrorMode(uint uMode);

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint ShellExecute(nint hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

    internal static void SetErrorMode() => SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX | SEM_NOALIGNMENTFAULTEXCEPT);

    internal static void ShellExecute(string lpFile)
    {
        nint hWnd = 0;

        if (Application.Current?.MainWindow is { } window)
        {
            WindowInteropHelper helper = new(window);
            hWnd = helper.EnsureHandle();
        }

        ShellExecute(hWnd, null!, lpFile, null!, null!, SW_NORMAL);
    }
}