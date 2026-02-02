using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static System.IO.Directory;
using static System.IO.NotifyFilters;
using static System.Environment;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using Windows.ApplicationModel;
using System.ComponentModel;
using System.Diagnostics;
using System;

namespace Flarial.Launcher.Services.Game;

using static System.NativeProcess;

unsafe sealed class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base() { }
    protected override string WindowClass => "Bedrock";

    /*
        - Using Windows Powershell's binary directly, makes the code more portable.
        - The `System.Management.Automation` package for Windows PowerShell is only for .NET Framework.
    */

    static readonly string s_path = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), @"Minecraft Bedrock\Users");
    static readonly string s_filename = Path.Combine(GetFolderPath(SpecialFolder.System), @"WindowsPowerShell\v1.0\powershell.exe");
    static readonly string s_arguments = $"-ExecutionPolicy \"Bypass\" -NoProfile -NonInteractive -Command \"Invoke-CommandInDesktopPackage '{PackageFamilyName}' 'App' '{{0}}'\"";

    protected override uint? Activate()
    {
        /*
            - Verify if the game is actually signed by the Microsoft Store.
            - This allows the launcher ensure the launch contract works as intended.
        */

        if (!IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!AllowUnsignedInstalls && !IsPackaged)
            throw new Win32Exception((int)ERROR_SERVICE_EXISTS_AS_NON_PACKAGED_SERVICE);

        /*
            - We use PowerShell to directly start the game executable.
            - This bypasses the PC Bootstrapper (GDK), simplifying the launch process.
        */

        if (GetProcessId() is { } processId)
            return processId;

        var path = Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe");
        if (!File.Exists(path)) throw new FileNotFoundException();

        using var process = Process.Start(new ProcessStartInfo()
        {
            FileName = s_filename,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = string.Format(s_arguments, path),
        }); process.WaitForExit();

        return GetProcessId();
    }

    public override uint? Launch(bool initialized)
    {
        if (Window is { } window)
        {
            window.Switch();
            return window.ProcessId;
        }

        if (Activate() is not { } processId)
            return null;

        if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process)
            return null;

        /*
            - The initialization logic is derived from the UWP builds of the game.
            - We don't need to resort to polling since symbolic links aren't used.
        */

        using (process)
        {
            var @event = CreateEvent(null, true, false, null); try
            {
                using FileSystemWatcher watcher = new(CreateDirectory(s_path).FullName, initialized ? "*resource_init_lock" : "*menu_load_lock")
                {
                    InternalBufferSize = 0,
                    NotifyFilter = FileName,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true
                };

                /*
                    - Use unmanaged events directly.
                    - This removes the need for boilerplate code.
                */

                watcher.Deleted += (_, _) => SetEvent(@event);
                var handles = stackalloc HANDLE[] { @event, process };

                return WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0 ? processId : null;
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

                        if (CompareStringOrdinal(name, -1, entry.pProcessName, -1, true) is not CSTR_EQUAL)
                            continue;

                        if (Open(PROCESS_QUERY_LIMITED_INFORMATION, entry.ProcessId) is not { } process)
                            continue;

                        using (process)
                        {
                            if (GetPackageFamilyName(process, &length, buffer) != ERROR_SUCCESS)
                                continue;

                            if (CompareStringOrdinal(pfn, -1, buffer, -1, true) != CSTR_EQUAL)
                                continue;

                            return entry.ProcessId;
                        }
                    }
                return null;
            }
            finally { WTSFreeMemoryEx(WTSTypeProcessInfoLevel0, information, count); }
        }
    }
}