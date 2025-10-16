using System.IO;
using Flarial.Launcher.Services.System;
using Windows.Management.Core;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftUWP : Minecraft
{
    internal MinecraftUWP() : base("Microsoft.MinecraftUWP_8wekyb3d8bbwe!App") { }
}

unsafe partial class MinecraftUWP
{
    public override bool IsRunning
    {
        get
        {
            fixed (char* @class = "MSCTFIME UI")
            fixed (char* string1 = _applicationUserModelId)
            {
                Win32Window window = HWND.Null;
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    using var process = window.Process;

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
    public override uint? LaunchGame(bool initialized)
    {
        if (IsRunning) return ActivateApplication();

        var path1 = ApplicationDataManager.CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var path2 = initialized ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        fixed (char* path = Path.Combine(path1, path2))
        {
            Win32File? file = null; try
            {
                var processId = ActivateApplication();
                using Win32Process process = new(ActivateApplication());

                while (process.IsRunning(1))
                {
                    file ??= Win32File.Open(path);
                    if (file?.IsDeleted ?? false) return processId;
                }

                return null;
            }
            finally { file?.Dispose(); }
        }
    }
}

unsafe partial class MinecraftUWP
{
    public override void TerminateGame()
    {
        uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
        PWSTR string1 = new(), string2 = stackalloc char[(int)length];

        GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);
        s_packageDebugSettings.TerminateAllProcesses(string2);
    }
}