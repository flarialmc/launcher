using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Management.Core.ApplicationDataManager;
using static Flarial.Launcher.Services.System.Win32Process;

namespace Flarial.Launcher.Services.Core;

unsafe sealed class MinecraftUWP : Minecraft
{
    protected override string WindowClass => "MSCTFIME UI";
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App";
    internal MinecraftUWP() : base() { }

    /*
        - Every UWP window has a "MSCTFIME UI" window that is a child of the desktop window.
        - This is useful since we don't account for parent windows.
    */

    public override uint? Launch(bool initialized)
    {
        if (IsRunning) return Activate();
        var path1 = CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var path2 = initialized ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        /*
            - Here, we poll for changes to ensure the game initialized.
            - Due to symlinks, we must resort to polling for UWP builds.
        */

        fixed (char* path = Path.Combine(path1, path2))
        {
            if (Activate() is not { } processId) return null;
            if (Open(PROCESS_SYNCHRONIZE, processId) is not { } process) return null;

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