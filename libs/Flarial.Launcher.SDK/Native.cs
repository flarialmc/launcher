using System.Runtime.InteropServices;
using System.Security;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Flarial.Launcher.SDK;

[SuppressUnmanagedCodeSecurity]
static class Native
{
    internal const uint ERROR_ACCESS_DISABLED_WEBBLADE_TAMPER = 0x000004FE;

    internal const int PROCESS_ALL_ACCESS = 0X1FFFFF;

    internal const int DUPLICATE_CLOSE_SOURCE = 0x00000001;

    internal const int DUPLICATE_SAME_ACCESS = 0x00000002;

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern bool DuplicateHandle(nint hSourceProcessHandle, nint hSourceHandle, nint hTargetProcessHandle, out nint lpTargetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);

    [DllImport("Kernel32", CharSet = CharSet.Unicode, EntryPoint = "CreateMutexW", ExactSpelling = true, SetLastError = true)]
    internal static extern nint CreateMutex(nint lpMutexAttributes, bool bInitialOwner, string lpName);

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern nint GetCurrentProcess();

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern bool CloseHandle(nint hObject);

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern int GetCurrentProcessId();

    [DllImport("WSClient.dll"), PreserveSig]
    internal static extern int CheckDeveloperLicense(out nint pExpiration);

    [DllImport("WSClient.dll")]
    internal static extern void RemoveDeveloperLicense(nint hwndParent);

    [DllImport("User32.dll")]
    internal static extern nint GetDesktopWindow();

    [DllImport("User32.dll")]
    internal static extern nint GetShellWindow();

    [DllImport("User32.dll")]
    internal static extern int GetWindowThreadProcessId(nint hWnd, out int lpdwProcessId);
}