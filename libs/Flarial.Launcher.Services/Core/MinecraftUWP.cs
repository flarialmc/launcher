using System;
using System.IO;
using System.Linq;
using Flarial.Launcher.Services.System;
using Windows.Management.Core;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftUWP : Minecraft
{
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App";

    internal MinecraftUWP() : base() { }
}

unsafe partial class MinecraftUWP
{
    /*
        - Every UWP window has a "MSCTFIME UI" window that is a child of the desktop window.
        - This is useful since we don't account for parent windows.
    */

    public override bool IsRunning => FindWindow("MSCTFIME UI") is { };

    public override uint? Launch(bool initialized)
    {
        if (IsRunning) return Activate();

        var path1 = ApplicationDataManager.CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var path2 = initialized ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        /*
            - Here, we poll for changes to ensure the game initialized.
            - Due to symlinks, we must resort to polling for UWP builds.
        */

        fixed (char* path = Path.Combine(path1, path2))
        {
            var processId = Activate();

            if (Win32Process.Open(PROCESS_SYNCHRONIZE, processId) is not { } process)
                return null;

            using (process)
            {
                while (GetFileAttributes(path) is INVALID_FILE_ATTRIBUTES)
                    if (!process.Wait(1)) return null;

                while (GetFileAttributes(path) is not INVALID_FILE_ATTRIBUTES)
                    if (!process.Wait(1)) return null;

                return processId;
            }
        }
    }
}