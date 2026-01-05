using System.Runtime.InteropServices;

namespace Flarial.Launcher;

[System.Security.SuppressUnmanagedCodeSecurity]
static class PInvoke
{
    internal const int SW_NORMAL = 1;

    [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern nint ShellExecute(nint hWnd, [Optional] string lpOperation, string lpFile, [Optional] string lpParameters, [Optional] string lpDirectory, int nShowCmd);
}