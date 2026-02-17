using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.System.Diagnostics.Debug.THREAD_ERROR_MODE;

namespace Flarial.Launcher;

[System.Security.SuppressUnmanagedCodeSecurity]
unsafe static class NativeMethods
{
    //  const int SW_NORMAL = 1;
    //   const uint SEM_NOGPFAULTERRORBOX = 0x0002;
    //  const uint SEM_FAILCRITICALERRORS = 0x0001;
    //  const uint SEM_NOOPENFILEERRORBOX = 0x8000;
    //  const uint SEM_NOALIGNMENTFAULTEXCEPT = 0x0004;

    //    [DllImport("Kernel32", SetLastError = true)]
    //  static extern uint SetErrorMode(uint uMode);

    //  [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    //  static extern nint ShellExecute(nint hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

    //  internal static void SetErrorMode() => SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX | SEM_NOALIGNMENTFAULTEXCEPT);

    internal static void SetErrorMode()
    {
        PInvoke.SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX | SEM_NOALIGNMENTFAULTEXCEPT);
    }

    internal static void ShellExecute(string @object)
    {
        HWND hWnd = HWND.Null;

        if (Application.Current?.MainWindow is { } window)
        {
            WindowInteropHelper helper = new(window);
            hWnd = (HWND)helper.EnsureHandle();
        }

        fixed (char* lpFile = @object)
            PInvoke.ShellExecute(hWnd, null, lpFile, null, null, SHOW_WINDOW_CMD.SW_SHOWNORMAL);
    }
}