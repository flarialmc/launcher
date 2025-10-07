using Flarial.Launcher.Services.System;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Core;

public unsafe abstract class Minecraft
{
    private protected static readonly IPackageDebugSettings Settings;

    private protected static readonly IApplicationActivationManager Manager;

    protected readonly string PackageFamilyName, ApplicationUserModelId;

    static Minecraft()
    {
        Settings = (IPackageDebugSettings)new PackageDebugSettings();
        Manager = (IApplicationActivationManager)new ApplicationActivationManager();
    }

    internal Minecraft(string packageFamilyName, string applicationUserModelId)
    {
        PackageFamilyName = packageFamilyName;
        ApplicationUserModelId = applicationUserModelId;
    }

    protected uint Activate()
    {
        fixed (char* appUserModelId = ApplicationUserModelId)
        {
            const ACTIVATEOPTIONS options = ACTIVATEOPTIONS.AO_NOERRORUI;
            Manager.ActivateApplication(appUserModelId, null, options, out var processId);
            return processId;
        }
    }

    public uint? Launch(bool @lock)
    {
        using var process = Get(@lock);
        return process.Running(0) ? process.Id : null;
    }

    internal abstract Win32Process Get(bool @lock);

    public abstract void Terminate();

    public bool Installed
    {
        get
        {
            uint count = new(), length = new();
            var error = GetPackagesByPackageFamily(PackageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_SUCCESS && count > 0;
        }
    }

    public abstract bool Running { get; }

    public bool Debug
    {
        set
        {
            uint count = 1, length = PACKAGE_FULL_NAME_MAX_LENGTH;
            PWSTR string1 = new(), string2 = stackalloc char[(int)length];

            GetPackagesByPackageFamily(PackageFamilyName, ref count, &string1, ref length, string2);
            if (value) Settings.EnableDebugging(string2, null, null); else Settings.DisableDebugging(string2);
        }
    }
}