using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.IO;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Flarial.Launcher.Services.System.Win32Process;
using static System.IO.Directory;
using static System.IO.NotifyFilters;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher.Services.Core;

unsafe class MinecraftGDK : Minecraft
{
    protected override string WindowClass => "Bedrock";
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!Game";
    static readonly string s_path = Path.Combine(GetFolderPath(ApplicationData), @"Minecraft Bedrock\Users");
    internal MinecraftGDK() : base() { }

    protected override uint? Activate()
    {
        if ((GetProcessId("GameLaunchHelper.exe") ?? base.Activate()) is not { } processId) return null;
      
        if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process) return null;
        using (process) process.WaitForExit();
      
        return Window?.ProcessId ?? GetProcessId("Minecraft.Windows.exe");
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

                return WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0 ? process.Id : null;
            }
            finally { CloseHandle(@event); }
        }
    }
}