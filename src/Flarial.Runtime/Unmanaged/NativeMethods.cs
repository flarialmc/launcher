using Windows.Win32;

using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Runtime.Unmanaged;

public unsafe static class NativeMethods
{
    public static void ShellExecute(string value)
    {
        fixed (char* file = value)
            PInvoke.ShellExecute(lpFile: file, nShowCmd: SW_NORMAL);
    }
}