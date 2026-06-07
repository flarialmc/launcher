using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Runtime.Unmanaged;

public unsafe static class NativePlatform
{
    public static void Open(string target)
    {
        fixed (char* value = target)
            ShellExecute(lpFile: value, nShowCmd: SW_NORMAL);
    }
}