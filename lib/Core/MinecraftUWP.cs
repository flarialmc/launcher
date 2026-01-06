using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Management.Core.ApplicationDataManager;
using Windows.Win32.UI.Shell;

namespace Flarial.Launcher.Services.Core;

using static System.NativeProcess;

unsafe sealed class MinecraftUWP : Minecraft
{
    internal MinecraftUWP() : base() { }
    protected override string WindowClass => "MSCTFIME UI";

    static readonly IPackageDebugSettings s_package = (IPackageDebugSettings)new PackageDebugSettings();
    static readonly IApplicationActivationManager s_application = (IApplicationActivationManager)new ApplicationActivationManager();

    /*
        - We request the OS to start the game & return its process identifier.
        - Alongside, we also enable debug mode on the game's app package to prevent PLM from kicking in.
    */

    protected override uint? Activate()
    {
        fixed (char* pfn = Package.Id.FullName)
        fixed (char* aumid = "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App")
        {
            s_package.EnableDebugging(pfn, null, null);
            s_application.ActivateApplication(aumid, null, ACTIVATEOPTIONS.AO_NONE, out var processId);
            return processId;
        }
    }

    /*
        - We should wait for the game to fully initialize.
        - The user can alter this behave but might lead to crashes.
        - This ensures the game doesn't crash when injecting any mods.
    */

    public override uint? Launch(bool initialized)
    {
        if (IsRunning) return Activate();
        var parent = CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var child = initialized ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        fixed (char* path = Path.Combine(parent, child))
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