using System;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.HANDLE;
using Windows.Win32.Globalization;
using System.IO;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftGDK : Minecraft
{
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game";

    static readonly string s_path;

    internal MinecraftGDK() : base() { }

    static MinecraftGDK()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        s_path = Path.Combine(path, @"Minecraft Bedrock\Users");
    }
}

unsafe partial class MinecraftGDK
{
    uint LaunchBootstrapper()
    {
        fixed (char* processName = "GameLaunchHelper.exe")
        fixed (char* applicationUserModelId = ApplicationUserModelId)
        {
            uint level = 0, count = 0, length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            WTS_PROCESS_INFOW* information = null;
            var @string = stackalloc char[(int)length];

            try
            {
                if (WTSEnumerateProcessesEx(WTS_CURRENT_SERVER_HANDLE, &level, WTS_CURRENT_SESSION, (PWSTR*)&information, &count))
                    for (var index = 0; index < count; index++)
                    {
                        var entry = information[index];

                        var result = CompareStringOrdinal(processName, -1, entry.pProcessName, -1, true);
                        if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                        using Win32Process process = new(entry.ProcessId);

                        var error = GetApplicationUserModelId(process, &length, @string);
                        if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                        result = CompareStringOrdinal(applicationUserModelId, -1, @string, -1, true);
                        if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                        return entry.ProcessId;
                    }

                return ActivateApplication();
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }

    Win32Window? FindWindow()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = ApplicationUserModelId)
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
    public override bool IsRunning => FindWindow() is { };

    public override void Terminate() => FindWindow()?.Close();

    public override uint? Launch(bool initialized)
    {
        if (FindWindow() is { } running)
        {
            running.Switch();
            return running.ProcessId;
        }

        using (Win32Process bootstrapper = new(LaunchBootstrapper()))
            bootstrapper.IsRunning(INFINITE);

        if (FindWindow() is not { } @new)
            return null;

        using Win32Event @event = new();
        using Win32Process process = new(@new.ProcessId);

        using FileSystemWatcher watcher = new(s_path, initialized ? "*resource_init_lock" : "*menu_load_lock")
        {
            InternalBufferSize = 0,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
        }; watcher.Deleted += delegate { @event.Set(); };

        var handles = stackalloc HANDLE[] { @event, process };
        if (WaitForMultipleObjects(2, handles, false, INFINITE) > 0) return null;

        @new.Switch(); return @new.ProcessId;
    }
}