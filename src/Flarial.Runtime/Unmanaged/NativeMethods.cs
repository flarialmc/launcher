using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using static Windows.Win32.UI.WindowsAndMessaging.MESSAGEBOX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Runtime.Unmanaged;

public unsafe static class NativeMethods
{
    public static void ShellExecute(in ReadOnlySpan<char> file)
    {
        fixed (char* lpFile = file)
            PInvoke.ShellExecute(lpFile: lpFile, nShowCmd: SW_NORMAL);
    }

    public static void MessageBox(in nint handle, in ReadOnlySpan<char> text, in ReadOnlySpan<char> caption)
    {
        fixed (char* lpText = text)
        fixed (char* lpCaption = caption)
        {
            HWND hWnd = new(handle);
            PInvoke.MessageBox(hWnd, lpText, lpCaption, MB_ICONERROR);
        }
    }
}