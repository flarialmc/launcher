using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

[System.Security.SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    const int SW_NORMAL = 1;

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint ShellExecute(nint hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

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