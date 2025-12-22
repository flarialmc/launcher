using System.Runtime.InteropServices;
using System.Security;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Flarial.Launcher.Services.SDK;

[SuppressUnmanagedCodeSecurity]
public static class Developer
{
    [DllImport("WSClient"), PreserveSig]
    internal static extern int CheckDeveloperLicense(out nint pExpiration);

    [DllImport("WSClient")]
    internal static extern void RemoveDeveloperLicense(nint hwndParent);

    public static bool Enabled => CheckDeveloperLicense(out _) == default;

    public static void Request() => RemoveDeveloperLicense(default);
}