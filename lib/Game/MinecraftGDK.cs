using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using Windows.ApplicationModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;
using Flarial.Launcher.Services.System;
using System;
using System.ComponentModel;

namespace Flarial.Launcher.Services.Game;

using static System.NativeProcess;

unsafe sealed class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base() { }
    protected override string Class => "Bedrock";

    static MinecraftGDK()
    {
        /*
            - Initialize a minimal PowerShell session.
            - Avoid defaults since they might change.
        */

        s_state.ImportPSModule(["Appx"]);
        s_state.LanguageMode = PSLanguageMode.NoLanguage;
        s_state.ExecutionPolicy = ExecutionPolicy.Restricted;
        s_state.ThreadOptions = PSThreadOptions.UseCurrentThread;
    }

    static readonly InitialSessionState s_state = InitialSessionState.Create();
    static readonly string s_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Minecraft Bedrock\Users");

    protected override uint? Activate()
    {
        /*
            - Unlike UWP builds, we are bootstrapping the game manually.
            - Verify if the game is installed & Gaming Services is available.
        */

        if (!IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!IsGamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

        /*
            - Unsigned installs might fail the activation contract.
            - Prefer to fail unless unsigned installs are allowed.
        */

        if (!(bool)AllowUnsignedInstalls!) if (!IsPackaged)
            throw new Win32Exception((int)ERROR_SERVICE_EXISTS_AS_NON_PACKAGED_SERVICE);

        var path = Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe");

        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);

        if (GetProcessId() is { } processId)
            return processId;

        /*
            - We use PowerShell to directly start the game.
            - This simplifies the activation contract.
        */

        using var powershell = PowerShell.Create(s_state);
        powershell.AddCommand("Invoke-CommandInDesktopPackage");

        powershell.AddParameter("AppId", "Game");
        powershell.AddParameter("Command", path);
        powershell.AddParameter("PackageFamilyName", PackageFamilyName);

        powershell.Invoke();
        return GetProcessId();
    }

    public override uint? Launch(bool initialized)
    {
        if (GetWindow() is { } window)
        {
            window.Switch();
            return window.ProcessId;
        }

        if (Activate() is not { } processId) return null;
        if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process) return null;

        using (process)
        {
            /*
                - The initialization logic is derived from the UWP builds of the game.
                - We don't need to resort to polling since symbolic links aren't used.
            */

            var @event = CreateEvent(null, true, false, null); try
            {
                var path = Directory.CreateDirectory(s_path).FullName;
                using FileSystemWatcher watcher = new(path, initialized ? "*resource_init_lock" : "*menu_load_lock")
                {
                    InternalBufferSize = 0,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                };

                watcher.Deleted += (_, _) => SetEvent(@event);
                var handles = stackalloc HANDLE[] { @event, process };

                if (WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0)
                    return processId;

                return null;
            }
            finally { CloseHandle(@event); }
        }
    }

    uint? GetProcessId()
    {
        fixed (char* pfn = PackageFamilyName)
        fixed (char* name = "Minecraft.Windows.exe")
        {
            uint level = 0, count = 0, length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
            WTS_PROCESS_INFOW* information = null;
            var buffer = stackalloc char[(int)length];

            try
            {
                if (WTSEnumerateProcessesEx(WTS_CURRENT_SERVER_HANDLE, &level, WTS_CURRENT_SESSION, (PWSTR*)&information, &count))
                    for (var index = 0; index < count; index++)
                    {
                        var entry = information[index];
                        if (CompareStringOrdinal(name, -1, entry.pProcessName, -1, true) is not CSTR_EQUAL) continue;
                        if (Open(PROCESS_QUERY_LIMITED_INFORMATION, entry.ProcessId) is not { } process) continue;

                        using (process)
                        {
                            if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS) continue;
                            if (CompareStringOrdinal(pfn, -1, buffer, -1, true) != CSTR_EQUAL) continue;
                            return entry.ProcessId;
                        }
                    }
                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }
}