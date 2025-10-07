using System.Diagnostics;
using Windows.Win32.UI.Shell;

namespace Flarial.Launcher.Services.Core;

public abstract class Game
{
    private protected static readonly IPackageDebugSettings Settings;

    private protected static readonly IApplicationActivationManager Manager;

    private protected string PackageFamilyName;

    private protected string ApplicationUserModelId;

    static Game()
    {
        Settings = (IPackageDebugSettings)new PackageDebugSettings();
        Manager = (IApplicationActivationManager)new ApplicationActivationManager();
    }

    internal Game(string packageFamilyName, string applicationUserModelId)
    {
        PackageFamilyName = packageFamilyName;
        ApplicationUserModelId = applicationUserModelId;
    }

    public abstract uint? Launch();

    public abstract void Terminate();

    public bool Installed
    {
        get
        {
            return false;
        }
    }

    public abstract bool Running { get; }

    public bool Debug
    {
        set
        {

        }
    }
}