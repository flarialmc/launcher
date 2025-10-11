using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract partial class Minecraft
{
    public static readonly Minecraft UWP = new MinecraftUWP();
}

partial class Minecraft
{
    protected const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    private protected static readonly IPackageDebugSettings _settings = (IPackageDebugSettings)new PackageDebugSettings();

    static readonly IApplicationActivationManager _manager = (IApplicationActivationManager)new ApplicationActivationManager();
}

partial class Minecraft
{
    protected readonly string ApplicationUserModelId;

    internal Minecraft(string applicationUserModelId) => ApplicationUserModelId = applicationUserModelId;
}

unsafe partial class Minecraft
{
    public uint? LaunchGame(bool initialized)
    {
        using var process = BootstrapGame(initialized);
        return process.IsRunning(0) ? process.Id : null;
    }

    private protected Win32Process ActivateApplication()
    {
        fixed (char* appUserModelId = ApplicationUserModelId)
        {
            const ACTIVATEOPTIONS options = ACTIVATEOPTIONS.AO_NOERRORUI;
            _manager.ActivateApplication(appUserModelId, null, options, out var processId);
            return new(processId);
        }
    }

    public bool HasUWPAppLifecycle
    {
        set
        {
            uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
            PWSTR string1 = new(), string2 = stackalloc char[(int)length];

            GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);
            if (value) _settings.EnableDebugging(string2, null, null);
            else _settings.DisableDebugging(string2);
        }
    }
}

unsafe partial class Minecraft
{
    public static bool IsInstalled
    {
        get
        {
            uint count = new(), length = new();
            var error = GetPackagesByPackageFamily(PackageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_SUCCESS && count > 0;
        }
    }
}

partial class Minecraft
{
    public abstract void TerminateGame();

    public abstract bool IsRunning { get; }

    internal abstract Win32Process BootstrapGame(bool initialized);
}