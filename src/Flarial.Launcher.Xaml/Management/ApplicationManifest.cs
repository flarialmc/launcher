using System.IO;
using System.Reflection;

namespace Flarial.Launcher.Management;

static class ApplicationManifest
{
    static readonly Assembly s_assembly;

    internal static readonly string s_version;

    static ApplicationManifest()
    {
        s_assembly = Assembly.GetExecutingAssembly();
        s_version = $"{s_assembly.GetName().Version}";
    }

    internal static Stream GetResourceStream(string name) => s_assembly.GetManifestResourceStream(name);
}