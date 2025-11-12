using System;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftGDK : Minecraft
{
    protected override string WindowClass => "Bedrock";

    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game";

    static readonly string s_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Minecraft Bedrock\Users");

    internal MinecraftGDK() : base() { }
}

unsafe partial class MinecraftGDK
{
    public override uint? Launch(bool initialized)
    {
        if (Window is { } window1)
        {
            window1.Switch();
            return window1.ProcessId;
        }

        /* 
            - Attempt to find the "PC Bootstrapper" process & wait for it to exit.
            - The process will automatically close once the game window is visible.
        */

        var processId = GetProcessId("GameLaunchHelper.exe") ?? Activate();
        if (Win32Process.Open(PROCESS_SYNCHRONIZE, processId) is not { } process1) return null;

        using (process1) process1.Wait(INFINITE);

        /*
            - The "PC Bootstrapper" process will close once any game window is visible.
            - This also includes potential miscellaneous windows from other sources which is undesirable.
            - If we cannot find the desired window then fallback to finding the actual process.
        */

        processId = Window?.ProcessId ?? GetProcessId("Minecraft.Windows.exe") ?? 0;
        if (Win32Process.Open(PROCESS_SYNCHRONIZE, processId) is not { } process2) return null;

        /*
            - GDK builds store data in unique directories for each signed user.
            - Hence we must wildcard to ensure the game is actually initialized.
        */

        using (process2)
        {
            HANDLE @event = CreateEvent(null, true, false, null); try
            {
                using FileSystemWatcher watcher = new(s_path, initialized ? "*resource_init_lock" : "*menu_load_lock")
                {
                    InternalBufferSize = 0,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                };

                watcher.Deleted += (_, _) => SetEvent(@event);
                var handles = stackalloc HANDLE[] { @event, process2 };
                return WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0 ? processId : null;
            }
            finally { CloseHandle(@event); }
        }
    }
}