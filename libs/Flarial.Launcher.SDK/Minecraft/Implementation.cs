using System.Threading.Tasks;
using Flarial.Launcher.Services.Modding;


namespace Flarial.Launcher.SDK;

public static partial class Minecraft
{
    static readonly Injector s_injector = Injector.UWP;
    
    static readonly Services.Core.Minecraft s_minecraft = Services.Core.Minecraft.UWP;

    public static partial bool Installed => Services.Core.Minecraft.IsInstalled;

    public static partial bool Running => s_minecraft.IsRunning;

    public static partial bool Debug { set { Services.Core.Minecraft.HasUWPAppLifecycle = value; } }

    public static partial bool Launch() => s_minecraft.LaunchGame(true).HasValue;

    public static partial bool Launch(string path) => s_injector.LaunchGame(true, path).HasValue;

    public static partial void Terminate() => s_minecraft.TerminateGame();

    public static partial string Version => Services.Core.Minecraft.ClientVersion;

    public static partial bool Unpackaged => Services.Core.Minecraft.IsUnpackaged;

    public static async partial Task<bool> LaunchAsync(string path) => await Task.Run(() => Launch(path));

    public static partial bool GDK => Services.Core.Minecraft.UsingGameDevelopmentKit;
}