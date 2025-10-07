using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using static Windows.Wdk.System.Threading.PROCESSINFOCLASS;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Services.Core;

unsafe sealed class MinecraftWindowsBeta : Minecraft
{
    const string PackageFamilyName = "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";

    const string ApplicationModelUserId = $"{PackageFamilyName}!Game";

    internal MinecraftWindowsBeta() : base(PackageFamilyName, ApplicationModelUserId) { }

    public override bool IsRunning
    {
        get
        {
            if (FindGameProcess() is not { } process) return false;
            using (process) return process.IsRunning(0);
        }
    }

    ProcessHandle? FindGameProcess()
    {
        fixed (char* @class = "Bedrock")
        fixed (char* string1 = _applicationModelUserId)
        {
            HWND window = HWND.Null;
            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                uint processId = 0;
                GetWindowThreadProcessId(window, &processId);
                if (ProcessHandle.Open(processId) is not { } process) continue;

                var error = GetApplicationUserModelId(process, &length, string2);
                if (error is not ERROR_SUCCESS) using (process) continue;

                var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                if (result is not CSTR_EQUAL) using (process) continue;

                return process;
            }

            return null;
        }
    }

    bool LaunchBootstrapperProcess()
    {
        fixed (char* string1 = _applicationModelUserId)
        fixed (char* @class = "GAMINGSERVICESUI_HOSTING_WINDOW_CLASS")
        {
            HWND window = HWND.Null;
            PROCESS_BASIC_INFORMATION information = new();

            var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
            var string2 = stackalloc char[(int)length];

            while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
            {
                if (ProcessHandle.Open(window) is not { } process1) continue;
                using (process1) NtQueryInformationProcess(process1, ProcessBasicInformation, &information, (uint)sizeof(PROCESS_BASIC_INFORMATION), null);

                var processId = (uint)information.InheritedFromUniqueProcessId;
                if (ProcessHandle.Open(processId) is not { } process2) continue;

                using (process2)
                {
                    var error = GetApplicationUserModelId(process2, &length, string2);
                    if (error is not ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                    if (result is not CSTR_EQUAL) continue;

                    return process2.WaitForExit();
                }
            }

            if (ProcessHandle.Open(Activate()) is not { } process3) return false;
            using (process3) return process3.WaitForExit();
        }
    }


    public override void Terminate()
    {
        if (FindGameProcess() is not { } process) return;
        using (process) process.Terminate();
    }

    internal override ProcessHandle? LaunchProcess(LaunchType type)
    {
        if (!LaunchBootstrapperProcess()) return null;
        return FindGameProcess();
    }
}