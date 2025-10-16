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

    private protected static readonly IPackageDebugSettings s_packageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();

    static readonly IApplicationActivationManager s_applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();
}

partial class Minecraft
{
    protected readonly string _applicationUserModelId;

    internal Minecraft(string applicationUserModelId) => _applicationUserModelId = applicationUserModelId;
}

unsafe partial class Minecraft
{
    private protected uint ActivateApplication()
    {
        fixed (char* appUserModelId = _applicationUserModelId)
        {
            const ACTIVATEOPTIONS options = ACTIVATEOPTIONS.AO_NOERRORUI;
            s_applicationActivationManager.ActivateApplication(appUserModelId, null, options, out var processId);
            return processId;
        }
    }

    public bool HasUWPAppLifecycle
    {
        set
        {
            uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
            PWSTR string1 = new(), string2 = stackalloc char[(int)length];

            GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);
            if (value) s_packageDebugSettings.EnableDebugging(string2, null, null);
            else s_packageDebugSettings.DisableDebugging(string2);
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

unsafe partial class Minecraft
{
    public static bool UsingGameDevelopmentKit
    {
        get
        {
            
            return false;
        }
    }
}

partial class Minecraft
{
    public abstract uint? LaunchGame(bool initialized);

    public abstract void TerminateGame();

    public abstract bool IsRunning { get; }
}