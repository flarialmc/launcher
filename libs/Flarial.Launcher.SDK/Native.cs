using System.Runtime.InteropServices;
using System.Security;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Flarial.Launcher.SDK;

[SuppressUnmanagedCodeSecurity]
static class Native
{
    [DllImport("WSClient.dll"), PreserveSig]
    internal static extern int CheckDeveloperLicense(out nint pExpiration);

    [DllImport("WSClient.dll")]
    internal static extern void RemoveDeveloperLicense(nint hwndParent);
}