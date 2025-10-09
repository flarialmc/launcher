using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract class Minecraft
{
    public static readonly Minecraft UWP = new MinecraftUWP();

    private protected static readonly IPackageDebugSettings PackageDebugSettings;

    static readonly IApplicationActivationManager _applicationActivationManager;

    protected readonly string PackageFamilyName, ApplicationUserModelId;

    static Minecraft()
    {
        PackageDebugSettings = (IPackageDebugSettings)new PackageDebugSettings();
        _applicationActivationManager = (IApplicationActivationManager)new ApplicationActivationManager();
    }

    internal Minecraft(string packageFamilyName, string applicationUserModelId)
    {
        PackageFamilyName = packageFamilyName;
        ApplicationUserModelId = applicationUserModelId;
    }

    private protected Win32Process ActivateApplication()
    {
        fixed (char* appUserModelId = ApplicationUserModelId)
        {
            const ACTIVATEOPTIONS options = ACTIVATEOPTIONS.AO_NOERRORUI;
            _applicationActivationManager.ActivateApplication(appUserModelId, null, options, out var processId);
            return new(processId);
        }
    }

    public uint? LaunchGame(bool initialized)
    {
        using var process = BootstrapGame(initialized);
        return process.IsRunning(0) ? process.Id : null;
    }

    internal abstract Win32Process BootstrapGame(bool initialized);

    public abstract void TerminateGame();

    public bool IsInstalled
    {
        get
        {
            uint count = new(), length = new();
            var error = GetPackagesByPackageFamily(PackageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_SUCCESS && count > 0;
        }
    }

    public abstract bool IsRunning { get; }

    public bool HasUWPAppLifecycle
    {
        set
        {
            uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
            PWSTR string1 = new(), string2 = stackalloc char[(int)length];
            GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);

            if (value) PackageDebugSettings.EnableDebugging(string2, null, null);
            else PackageDebugSettings.DisableDebugging(string2);
        }
    }
}