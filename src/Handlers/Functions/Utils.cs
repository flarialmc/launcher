using System.Security.Principal;

namespace Flarial.Launcher.Functions;

public class Utils
{
    static readonly WindowsPrincipal principal = new(WindowsIdentity.GetCurrent());

    public static bool IsGameOpen() => SDK.Minecraft.Installed;

    public static bool IsAdministrator => principal.IsInRole(WindowsBuiltInRole.Administrator);
}
