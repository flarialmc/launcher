using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)

)]

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
[assembly: AssemblyTitle("Flarial Launcher")]
[assembly: AssemblyDescription("The official Launcher for Flarial Client.")]
[assembly: AssemblyCompany("Flarial")]
[assembly: AssemblyProduct("Launcher")]
[assembly: AssemblyCopyright("Copyright © Flarial 2025")]
[assembly: AssemblyTrademark("Flarial")]
[assembly: ComVisible(false)]

[assembly: Guid("c68ed574-e6d9-4558-909f-82ec97711f64")]

[assembly: AssemblyVersion("2.1.13.8")]