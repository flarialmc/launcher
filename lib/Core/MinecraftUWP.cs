using System.IO;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Management.Core.ApplicationDataManager;
using Windows.Win32.UI.Shell;

namespace Flarial.Launcher.Services.Core;

using static Native.NativeProcess;

unsafe sealed class MinecraftUWP : Minecraft
{
    internal MinecraftUWP() : base() { }
    protected override string WindowClass => "MSCTFIME UI";

    static readonly IPackageDebugSettings s_settings = (IPackageDebugSettings)new PackageDebugSettings();
    static readonly IApplicationActivationManager s_manager = (IApplicationActivationManager)new ApplicationActivationManager();

    protected override uint? Activate()
    {
        fixed (char* pfn = Package.Id.FullName)
        fixed (char* aumid = "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App")
        {
            s_settings.EnableDebugging(pfn, null, null);
            s_manager.ActivateApplication(aumid, null, ACTIVATEOPTIONS.AO_NONE, out var processId);
            return processId;
        }
    }

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