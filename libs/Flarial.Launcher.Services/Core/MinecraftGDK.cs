using System;
using System.Xml.Schema;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using static Windows.Wdk.System.Threading.PROCESSINFOCLASS;
using Windows.Win32.System.Threading;
using Windows.Win32.Globalization;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base("Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe!Game") { }
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
                using var process1 = window.Process;

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
                using var process = window.Process;

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

partial class MinecraftGDK
{
    public override bool IsRunning
    {
        get
        {
            if (FindGameWindow() is not { } window)
                return false;

            using var process = window.Process;
            return process.IsRunning(0);
        }
    }
}

partial class MinecraftGDK
{
    public override void TerminateGame()
    {
        if (FindGameWindow() is not { } window)
            return;
        
        using var process = window.Process;
        window.EndTask(); process.IsRunning(INFINITE);
    }
}


partial class MinecraftGDK
{
    public override uint? LaunchGame(bool _)
    {
        throw new NotImplementedException();
    }
}