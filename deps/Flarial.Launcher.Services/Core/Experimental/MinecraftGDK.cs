using System.IO;
using System.Management.Automation;
using Flarial.Launcher.Services.System;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Flarial.Launcher.Services.System.Win32Process;

namespace Flarial.Launcher.Services.Core.Experimental;

sealed class MinecraftGDK : Core.MinecraftGDK
{
    const string AppId = "Game";
    const string Executable = "Minecraft.Windows.exe";

    uint? ProcessId => GetProcessId(Executable);
    string Command => Path.Combine(Package.InstalledPath, Executable);

    protected override uint? Activate()
    {
        if (UseBootstrapper) return base.Activate();
        if (ProcessId is { } processId) return processId;

        using var _ = PowerShell.Create();
        _.AddCommand("Invoke-CommandInDesktopPackage");

        @_.AddParameter(nameof(AppId), AppId);
        @_.AddParameter(nameof(Command), Command);
        @_.AddParameter(nameof(PackageFamilyName), PackageFamilyName);

        _.Invoke(); return ProcessId;
    }
}