using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.Debug.THREAD_ERROR_MODE;

[assembly: AssemblyCompany("Flarial")]
[assembly: AssemblyProduct("Runtime")]
[assembly: AssemblyTitle("Flarial Runtime")]
[assembly: SupportedOSPlatform("windows10.0.19041.0")]
[assembly: AssemblyCopyright("Copyright © Flarial 2025 - 2026")]

file static class AssemblyInfo
{
    [ModuleInitializer]
    internal static void ModuleInitializer() => SetErrorMode(SEM_FAILCRITICALERRORS);
}