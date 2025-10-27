using System.Threading.Tasks;
using Flarial.Launcher.Services.Modding;


namespace Flarial.Launcher.SDK;

public static class Minecraft
{
    static readonly Injector s_injector = Injector.UWP;

    static readonly Services.Core.Minecraft s_minecraft = Services.Core.Minecraft.UWP;

    public static bool Installed => Services.Core.Minecraft.IsInstalled;

    public static bool Running => s_minecraft.IsRunning;

    public static bool Debug { set { Services.Core.Minecraft.HasUWPAppLifecycle = value; } }

    public static bool Launch() => s_minecraft.LaunchGame(true).HasValue;

    public static bool Launch(string path) => s_injector.LaunchGame(true, path).HasValue;

    public static void Terminate() => s_minecraft.TerminateGame();

    public static string Version => Services.Core.Minecraft.ClientVersion;

    public static bool Unpackaged => Services.Core.Minecraft.IsUnpackaged;

    public static async Task<bool> LaunchAsync(string path) => await Task.Run(() => Launch(path));

    public static bool GDK => Services.Core.Minecraft.UsingGameDevelopmentKit;
}