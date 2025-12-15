using System.IO;
using System.Management.Automation;
using Flarial.Launcher.Services.System;

namespace Flarial.Launcher.Services.Core.Experimental;

sealed class MinecraftGDK : Core.MinecraftGDK
{
    Win32Process? Process => Win32Process.Open(GetProcessId("Minecraft.Windows.exe"));

    public override uint? Launch(bool initialized)
    {
        if (!BypassPCBootstrapper)
            return base.Launch(initialized);

        if (Window is { } window)
        {
            window.SwitchToThisWindow();
            return window.ProcessId;
        }

        if (Process is { } process1)
        {
            process1.WaitForInputIdle();
            return process1.ProcessId;
        }

        var path = Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe");
        var parameters = (string[])[PackageFamilyName, "Game", path];

        using (var @_ = PowerShell.Create())
        {
            @_.AddCommand("Invoke-CommandInDesktopPackage");
            @_.AddParameters(parameters);
            @_.Invoke();
        }

        if (Process is not { } process2)
            return null;

        using (process2)
        {
            process2.WaitForInputIdle();
            return process2.ProcessId;
        }
    }
}