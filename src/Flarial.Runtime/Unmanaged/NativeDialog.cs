using System;
using Windows.Win32.UI.Controls;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.Controls.TASKDIALOG_COMMON_BUTTON_FLAGS;
using static Windows.Win32.UI.Controls.TASKDIALOG_FLAGS;

namespace Flarial.Runtime.Unmanaged;

public unsafe readonly ref struct NativeDialog
{
    public required readonly nint Handle { get; init; }
    public required readonly string Title { get; init; }
    public required readonly string Content { get; init; }
    public required readonly string Instruction { get; init; }
    public required readonly string Information { get; init; }

    public void Show()
    {
        fixed (char* title = Title)
        fixed (char* content = Content)
        fixed (char* instruction = Instruction)
        fixed (char* information = Information)
        {
            TASKDIALOGCONFIG config = new()
            {
                pszWindowTitle = title,
                pszMainInstruction = instruction,

                pszContent = content,
                pszExpandedInformation = information,

                hwndParent = new(Handle),
                cbSize = (uint)sizeof(TASKDIALOGCONFIG),

                dwCommonButtons = TDCBF_CLOSE_BUTTON,
                Anonymous1 = new() { pszMainIcon = TD_ERROR_ICON },
                dwFlags = TDF_SIZE_TO_CONTENT | TDF_ALLOW_DIALOG_CANCELLATION | TDF_POSITION_RELATIVE_TO_WINDOW
            };
            TaskDialogIndirect(&config);
        }
    }
}