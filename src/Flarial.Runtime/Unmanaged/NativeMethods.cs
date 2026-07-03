using Windows.Win32;
using Windows.Win32.UI.Controls;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.Controls.TASKDIALOG_COMMON_BUTTON_FLAGS;
using static Windows.Win32.UI.Controls.TASKDIALOG_FLAGS;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Runtime.Unmanaged;

public unsafe static class NativeMethods
{
    public static void ShellExecute(string file)
    {
        fixed (char* filePtr = file)
            PInvoke.ShellExecute(lpFile: filePtr, nShowCmd: SW_NORMAL);
    }

    public static void TaskDialog(nint handle, string title, string? instruction, string content, string? information)
    {
        fixed (char* titlePtr = title)
        fixed (char* instructionPtr = instruction)
        fixed (char* contentPtr = content)
        fixed (char* informationPtr = information)
        {
            TASKDIALOGCONFIG config = new()
            {
                pszWindowTitle = titlePtr,
                pszMainInstruction = instructionPtr,

                pszContent = contentPtr,
                pszExpandedInformation = informationPtr,

                hwndParent = new(handle),
                cbSize = (uint)sizeof(TASKDIALOGCONFIG),

                dwCommonButtons = TDCBF_CLOSE_BUTTON,
                Anonymous1 = new() { pszMainIcon = TD_ERROR_ICON },
                dwFlags = TDF_SIZE_TO_CONTENT | TDF_ALLOW_DIALOG_CANCELLATION | TDF_POSITION_RELATIVE_TO_WINDOW
            };
            TaskDialogIndirect(&config);
        }
    }
}