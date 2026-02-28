using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Directory;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Runtime.Game;

using static System.NativeProcess;

unsafe sealed class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base() { }

    protected override string Window => "Bedrock";
    protected override string Process => "Minecraft.Windows.exe";

    static MinecraftGDK()
    {
        s_state.ImportPSModule(["Appx"]);
        s_state.ThreadOptions = PSThreadOptions.UseCurrentThread;
    }

    static readonly InitialSessionState s_state = InitialSessionState.Create();
    static readonly string s_path = Path.Combine(GetFolderPath(ApplicationData), @"Minecraft Bedrock\Users");

    protected override uint? Activate()
    {
        var path = Path.Combine(Package.InstalledPath, Process);

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

        powershell.Invoke(); return GetProcessId();
    }

    public override uint? Launch(bool initialized)
    {
        /*
            - Unlike UWP builds, we are bootstrapping the game manually.
            - Hence, verify if the game & Gaming Services are installed.
        */

        if (!IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!IsGamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

        /*
            - Unsigned installs might fail the launch contract.
            - Prefer to fail unless unsigned installs are allowed.
        */

        if (!(bool)AllowUnsignedInstalls!) if (!IsPackaged)
            throw new Win32Exception((int)ERROR_SERVICE_EXISTS_AS_NON_PACKAGED_SERVICE);

        if (GetWindow() is { } window)
        {
            window.Switch();
            return window.ProcessId;
        }

        if (Activate() is not { } processId)
            return null;

        if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process)
            return null;

        using (process)
        {
            /*
                - The initialization logic is derived from the UWP builds of the game.
                - We don't need to resort to polling since symbolic links aren't used.
            */

            var @event = CreateEvent(null, true, false, null); try
            {
                using FileSystemWatcher watcher = new(CreateDirectory(s_path).FullName, initialized ? "*resource_init_lock" : "*menu_load_lock")
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
}