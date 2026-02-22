using System;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.Debug.THREAD_ERROR_MODE;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
namespace Flarial.Launcher;

[Obsolete]
static class NativeMethods
{
    unsafe internal static void ShellExecute(string value)
    {
        fixed (char* lpFile = value)
        {
            HWND handle = HWND.Null;

            if (Application.Current?.MainWindow is { } window)
            {
                WindowInteropHelper helper = new(window);
                handle = (HWND)helper.EnsureHandle();
            }

            PInvoke.ShellExecute(handle, null, lpFile, null, null, SW_SHOWNORMAL);
        }
    }
}