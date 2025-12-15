using System;
using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;

namespace Flarial.Launcher.Services.Core;

unsafe sealed class MinecraftGDK : Minecraft
{
    protected override string WindowClass => "Bedrock";
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game";
    static readonly string s_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Minecraft Bedrock\Users");
    internal MinecraftGDK() : base() { }

    public override uint? Launch(bool initialized)
    {
        if (Window is { } window) { window.Switch(); return window.ProcessId; }

        /* 
            - Attempt to find the "PC Bootstrapper" process & wait for it to exit.
            - The process will automatically close once the game window is visible.
        */

        var bootstrapperId = GetProcess("GameLaunchHelper.exe") ?? Activate();
        if (Win32Process.Open(PROCESS_SYNCHRONIZE, bootstrapperId) is not { } bootstrapper) return null;

        /*
            - The "PC Bootstrapper" process will close once any game window is visible.
            - This also includes potential miscellaneous windows from other sources which is undesirable.
            - If we cannot find the desired window then fallback to finding the actual process.
        */

        using (bootstrapper) bootstrapper.Wait(INFINITE);
        var gameId = Window?.ProcessId ?? GetProcess("Minecraft.Windows.exe") ?? 0;
        if (Win32Process.Open(PROCESS_SYNCHRONIZE, gameId) is not { } game) return null;

        /*
            - GDK builds store data in unique directories for each signed user.
            - Hence we must wildcard to ensure the game is actually initialized.
        */

        using (game)
        {
            HANDLE @event = CreateEvent(null, true, false, null); try
            {
                using FileSystemWatcher watcher = new(Directory.CreateDirectory(s_path).FullName, initialized ? "*resource_init_lock" : "*menu_load_lock")
                {
                    InternalBufferSize = 0,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                };

                watcher.Deleted += (_, _) => SetEvent(@event); var handles = stackalloc HANDLE[] { @event, game };
                return WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0 ? gameId : null;
            }
            finally { CloseHandle(@event); }
        }
    }
}