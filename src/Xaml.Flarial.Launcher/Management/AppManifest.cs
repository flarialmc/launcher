using System.IO;
using System.Reflection;

namespace Flarial.Launcher.Management;

static class AppManifest
{
    static readonly Assembly s_assembly;

    internal static readonly string s_version;

    static AppManifest()
    {
        s_assembly = Assembly.GetExecutingAssembly();
        s_version = $"{s_assembly.GetName().Version}";
    }

    internal static Stream GetStream(string name) => s_assembly.GetManifestResourceStream(name);
}