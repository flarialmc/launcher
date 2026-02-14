using System.Security.Principal;

namespace Flarial.Launcher.Functions;

static class Utils
{
    static Utils()
    {
        using var identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        IsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    internal static readonly bool IsAdministrator;
}