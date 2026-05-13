using System.Reflection;

namespace Flarial.Launcher.Management;

static class LauncherInfo
{
    static readonly Assembly s_assembly = Assembly.GetExecutingAssembly();

    internal static string Version { get; } = $"{s_assembly.GetName().Version}";
}
