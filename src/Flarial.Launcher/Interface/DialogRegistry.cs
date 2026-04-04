using Flarial.Launcher.Interface.Dialogs;

namespace Flarial.Launcher.Interface;

static class DialogRegistry
{
    internal static NotInstalledDialog NotInstalled => field ??= new();
    internal static LaunchFailureDialog LaunchFailure => field ??= new();
    internal static SelectVersionDialog SelectVersion => field ??= new();
    internal static InstallVersionDialog InstallVersion => field ??= new();
    internal static InvalidCustomDllDialog InvalidCustomDll => field ??= new();
    internal static ConnectionFailureDialog ConnectionFailure => field ??= new();
    internal static UnpackagedInstallDialog UnpackagedInstall => field ??= new();
    internal static ClientUpdateFailureDialog ClientUpdateFailure => field ??= new();
    internal static GamingServicesMissingDialog GamingServicesMissing => field ??= new();
    internal static LauncherUpdateAvailableDialog LauncherUpdateAvailable => field ??= new();
}