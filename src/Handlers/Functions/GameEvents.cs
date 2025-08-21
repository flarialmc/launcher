using System;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.OBJECT_IDENTIFIER;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Functions;

unsafe static partial class GameEvents
{
    const string ClassName = "Windows.UI.Core.CoreWindow";

    const string ApplicationUserModelId = "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App";

    static readonly char* _className;

    static readonly char* _applicationUserModelId;

    static GameEvents()
    {
        var handle = GCHandle.Alloc(ClassName, GCHandleType.Pinned);
        _className = (char*)handle.AddrOfPinnedObject();

        handle = GCHandle.Alloc(ApplicationUserModelId, GCHandleType.Pinned);
        _applicationUserModelId = (char*)handle.AddrOfPinnedObject();

        Hook(EVENT_OBJECT_CREATE, OnLaunched);
    }
}

partial class GameEvents
{
    internal static event Action Launched;
}

partial class GameEvents
{
    static bool OnEvent(uint @this, uint @event, int idObject, int idChild)
    {
        var flag = @this == @event;
        flag = flag && idObject is (int)OBJID_WINDOW;
        return flag && idChild is (int)CHILDID_SELF;
    }

    static void OnLaunched(HWINEVENTHOOK hWinEventHook, uint @event, HWND hWnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        var flag = OnEvent(EVENT_OBJECT_CREATE, @event, idObject, idChild);
        flag = flag && OnWindow(hWnd);
        if (flag) Launched.Invoke();
    }
}

partial class GameEvents
{
    const uint Flags = WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS;

    static void Hook(uint @event, WINEVENTPROC @delegate) => new Thread(() =>
    {
        var hook = SetWinEventHook(@event, @event, null, @delegate, 0, 0, Flags);
        if (hook.IsInvalid) throw new InvalidOperationException();
        while (GetMessage(out _, HWND.Null, 0, 0)) ;
    }).Start();
}

unsafe partial class GameEvents
{
    const int Count = 256;

    static bool OnWindow(HWND hWnd)
    {
        var className = stackalloc char[Count];
        GetClassName(hWnd, className, Count);

        var result = CompareStringOrdinal(_className, -1, className, -1, true);
        if (result is not CSTR_EQUAL) return false;

        var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
        var applicationUserModelId = stackalloc char[(int)length];

        uint dwProcessId = 0;
        GetWindowThreadProcessId(hWnd, &dwProcessId);

        using var process = OpenProcess_SafeHandle(PROCESS_QUERY_LIMITED_INFORMATION, false, dwProcessId);

        HANDLE handle = new(process.DangerousGetHandle());
        var error = GetApplicationUserModelId(handle, &length, applicationUserModelId);
        if (error is not ERROR_SUCCESS) return false;

        result = CompareStringOrdinal(_applicationUserModelId, -1, applicationUserModelId, -1, true);
        if (result is not CSTR_EQUAL) return false;

        return true;
    }
}