using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bedrockix.Minecraft;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Flarial.Launcher.SDK;

public static partial class Minecraft
{
    static readonly PackageManager _packageManager = new();

    const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    public static partial bool Installed => Game.Installed;

    public static partial bool Running => Game.Running;

    public static partial bool Debug { set { Game.Debug = value; } }

    public static partial bool Launch() => Game.Launch().HasValue;

    public static partial bool Launch(string path) => Loader.Launch(path).HasValue;

    public static partial void Terminate() => Game.Terminate();

    public static partial string Version => Metadata.Version;

    public static partial IEnumerable<Process> Processes => Metadata.Processes;

    public static partial bool Unpackaged => Game.Unpackaged;

    public static async partial Task<bool> LaunchAsync(string path) => await Task.Run(() => Launch(path));

    public static partial bool GDK
    {
        get
        {
            var package = _packageManager.FindPackagesForUser(string.Empty, PackageFamilyName).First();

            if (package.SignatureKind is not PackageSignatureKind.Store)
                return false;

            var path = Path.Combine(package.InstalledPath, "MicrosoftGame.config");
            return File.Exists(path);
        }
    }
}