
using System;
using System.IO;
using Flarial.Launcher.Services.System;
using Windows.Devices.Printers.Extensions;
using Windows.Management.Core;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

sealed partial class MinecraftUWP : Minecraft
{
    internal MinecraftUWP() : base("Microsoft.MinecraftUWP_8wekyb3d8bbwe", "Microsoft.MinecraftUWP_8wekyb3d8bbwe!App") { }
}


unsafe partial class MinecraftUWP
{
    public override bool Running
    {
        get
        {
            fixed (char* @class = "MSCTFIME UI")
            fixed (char* string1 = ApplicationUserModelId)
            {
                HWND window = HWND.Null;
                var length = APPLICATION_USER_MODEL_ID_MAX_LENGTH;
                var string2 = stackalloc char[(int)length];

                while ((window = FindWindowEx(HWND.Null, window, @class, null)) != HWND.Null)
                {
                    uint processId = 0;
                    GetWindowThreadProcessId(window, &processId);

                    using Win32Process process = new(processId);

                    var error = GetApplicationUserModelId(process, &length, string2);
                    if (error is not WIN32_ERROR.ERROR_SUCCESS) continue;

                    var result = CompareStringOrdinal(string1, -1, string2, 1, true);
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
    internal override Win32Process Get(bool @lock)
    {
        if (Running) return new(Activate());

        var path1 = ApplicationDataManager.CreateForPackageFamily(PackageFamilyName).LocalFolder.Path;
        var path2 = @lock ? @"games\com.mojang\minecraftpe\resource_init_lock" : @"games\com.mojang\minecraftpe\menu_load_lock";

        fixed (char* path = Path.Combine(path1, path2))
        {
            Win32File? file = null; try
            {
                Win32Process process = new(Activate());

                while (process.Running(1))
                {
                    file ??= Win32File.Open(path);
                    if (file?.Deleted ?? false) break;
                }

                return process;
            }
            finally { file?.Dispose(); }
        }
    }
}

unsafe partial class MinecraftUWP
{
    public override void Terminate()
    {
        throw new NotImplementedException();
    }
}