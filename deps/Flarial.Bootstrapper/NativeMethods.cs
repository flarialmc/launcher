using System.Runtime.InteropServices;
using System.Security;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

[SuppressUnmanagedCodeSecurity]
unsafe static class NativeMethods
{
    internal const uint PACKAGE_FAMILY_NAME_MAX_LENGTH = 64;

    internal const int CSTR_EQUAL = 2;

    internal const int ERROR_SUCCESS = 0x0;

    internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

    internal const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    [DllImport("User32", CharSet = CharSet.Unicode)]
    internal static extern void* FindWindowEx(void* hWndParent, void* hWndChildAfter, char* lpszClass, char* lpszWindow);

    [DllImport("Kernel32")]
    internal static extern int GetCurrentPackageFamilyName(uint* packageFamilyNameLength, char* packageFamilyName);

    [DllImport("User32")]
    internal static extern void SwitchToThisWindow(void* hwnd, [MarshalAs(UnmanagedType.Bool)] bool fUnknown);

    [DllImport("Kernel32")]
    internal static extern int GetPackageFamilyName(void* hProcess, uint* packageFamilyNameLength, char* packageFamilyName);

    [DllImport("User32")]
    internal static extern uint GetWindowThreadProcessId(void* hWnd, uint* lpdwProcessId);

    [DllImport("Kernel32")]
    internal static extern void* OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

    [DllImport("Kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle(void* hObject);

    [DllImport("Kernel32")]
    internal static extern int CompareStringOrdinal(char* lpString1, int cchCount1, char* lpString2, int cchCount2, [MarshalAs(UnmanagedType.Bool)] bool bIgnoreCase);
}