using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static System.IO.Directory;
using static System.IO.NotifyFilters;
using static System.Environment;
using static System.Environment.SpecialFolder;
using System.Management.Automation;
using Windows.Win32.System.RemoteDesktop;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.RemoteDesktop.WTS_TYPE_CLASS;
using Windows.ApplicationModel;
using System.ComponentModel;

namespace Flarial.Launcher.Services.Core;

using static Native.NativeProcess;

unsafe partial class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base() { }
    protected override string WindowClass => "Bedrock";

    const string AppId = "Game";
    string Command => Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe");
    static readonly string s_path = Path.Combine(GetFolderPath(ApplicationData), @"Minecraft Bedrock\Users");


    protected override uint? Activate()
    {
        /*
            - Verify if the game is actually signed by the Microsoft Store.
            - This allows the launcher ensure the launch contract works as intended.
        */

        if (!IsSigned)
            throw new Win32Exception((int)ERROR_SERVICE_EXISTS_AS_NON_PACKAGED_SERVICE);

        /*
            - We use PowerShell to directly start the game executable.
            - This bypasses the PC Bootstrapper (GDK), simplifying the launch process.
        */

        if (ProcessId is { } processId)
            return processId;

        using var _ = PowerShell.Create();
        _.AddCommand("Invoke-CommandInDesktopPackage");

        _.AddParameter(nameof(AppId), AppId);
        _.AddParameter(nameof(Command), Command);
        _.AddParameter(nameof(PackageFamilyName), PackageFamilyName);

        _.Invoke(); return ProcessId;
    }

    public override uint? Launch(bool initialized)
    {
        if (Window is { } window)
        {
            window.Switch();
            return window.ProcessId;
        }

        if (Activate() is not { } processId) return null;
        if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process) return null;

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
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = FileName
                };

                watcher.Deleted += (_, _) => SetEvent(@event);
                var handles = stackalloc HANDLE[] { @event, process };

                return WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0 ? processId : null;
            }
            finally { CloseHandle(@event); }
        }
    }

    uint? ProcessId
    {
        get
        {
            fixed (char* pfn = PackageFamilyName) fixed (char* name = "Minecraft.Windows.exe")
            {
                uint level = 0, count = 0, length = PACKAGE_FAMILY_NAME_MAX_LENGTH + 1;
                WTS_PROCESS_INFOW* information = null; var buffer = stackalloc char[(int)length];

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
}