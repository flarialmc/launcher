using System;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using static Windows.Wdk.System.Threading.PROCESSINFOCLASS;
using Windows.Win32.System.Threading;
using Windows.Win32.Globalization;
using System.IO;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftGDK : Minecraft
{
    const string ApplicationUserModelId = "Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game";

    static readonly string s_path;

    internal MinecraftGDK() : base(ApplicationUserModelId) { }

    static MinecraftGDK()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        s_path = Path.Combine(path, @"Minecraft Bedrock\Users");
    }
}

unsafe partial class MinecraftGDK
{
    internal uint FindBootstrapperProcess()
    {
        fixed (char* string1 = _applicationUserModelId)
        fixed (char* @class = "GAMINGSERVICESUI_HOSTING_WINDOW_CLASS")
        {
            Win32Window window = HWND.Null;
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                using Win32Process process1 = new(window.ProcessId);

                PROCESS_BASIC_INFORMATION information = new();
                NtQueryInformationProcess(process1, ProcessBasicInformation, &information, (uint)sizeof(PROCESS_BASIC_INFORMATION), null);

                var processId = (uint)information.InheritedFromUniqueProcessId;
                using Win32Process process2 = new(processId);

                var error = GetApplicationUserModelId(process2, &length, string2);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                return processId;
            }

            return ActivateApplication();
        }
    }

    internal Win32Window? FindGameWindow()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = _applicationUserModelId)
        {
            Win32Window window = new();
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                using Win32Process process = new(window.ProcessId);

                var error = GetApplicationUserModelId(process, &length, string2);
                if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                return window;
            }

            return null;
        }
    }
}

unsafe partial class MinecraftGDK
{
    public override bool IsRunning => FindGameWindow() is { };

    public override void TerminateGame() => FindGameWindow()?.EndTask();

    public override uint? LaunchGame(bool initialized)
    {
        if (FindGameWindow() is { } window1)
        {
            window1.SetForeground();
            return window1.ProcessId;
        }

        using Win32Process process1 = new(FindBootstrapperProcess());
        process1.IsRunning(INFINITE);

        if (FindGameWindow() is not { } window2)
            return null;

        using Win32Event @event = new();
        using Win32Process process = new(window2.ProcessId);

        using FileSystemWatcher watcher = new(s_path, initialized ? "*resource_init_lock" : "*menu_load_lock")
        {
            InternalBufferSize = new(),
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
        }; watcher.Deleted += delegate { @event.Set(); };

        var handles = stackalloc HANDLE[] { @event, process };
        if (WaitForMultipleObjects(2, handles, false, INFINITE) > 0) return null;

        return window2.ProcessId;
    }
}