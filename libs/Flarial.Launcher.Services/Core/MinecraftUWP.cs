using System.IO;
using System.Linq;
using Flarial.Launcher.Services.System;
using Windows.Management.Core;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftUWP : Minecraft
{
    protected override string ApplicationUserModelId => "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App";

    internal MinecraftUWP() : base() { }
}

unsafe partial class MinecraftUWP
{
    public override bool IsRunning
    {
        get
        {
            fixed (char* @class = "MSCTFIME UI")
            fixed (char* string1 = ApplicationUserModelId)
            {
                Win32Window window = HWND.Null;
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    using Win32Process process = new(window.ProcessId);

                    var error = GetApplicationUserModelId(process, &length, string2);
                    if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(string1, -1, string2, -1, true);
                    if (result is not COMPARESTRING_RESULT.CSTR_EQUAL) continue;

                    return true;
                }

                return false;
            }
        }
    }
}

unsafe partial class MinecraftUWP
{
    public override uint? Launch(bool initialized)
    {
        if (IsRunning) return ActivateApplication();

        var path1 = ApplicationDataManager.CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var path2 = initialized ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        fixed (char* path = Path.Combine(path1, path2))
        {
            var processId = ActivateApplication();
            using Win32Process process = new(processId);

            while (GetFileAttributes(path) is INVALID_FILE_ATTRIBUTES)
                if (!process.IsRunning(1)) return null;

            while (GetFileAttributes(path) is not INVALID_FILE_ATTRIBUTES)
                if (!process.IsRunning(1)) return null;

            return processId;
        }
    }
}

unsafe partial class MinecraftUWP
{
    public override void Terminate()
    {
        var package = s_packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();
        fixed (char* packageFullName = package.Id.FullName) s_packageDebugSettings.TerminateAllProcesses(packageFullName);
    }
}