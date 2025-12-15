using System.IO;
using System.Management.Automation;
using Flarial.Launcher.Services.System;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Services.Core.Experimental;

sealed class MinecraftGDK : Core.MinecraftGDK
{
    Win32Process? Process
    {
        get
        {
            if (GetProcessId("Minecraft.Windows.exe") is not { } processId) return null;
            return Win32Process.Open(PROCESS_QUERY_LIMITED_INFORMATION, processId);
        }
    }

    public override uint? Launch(bool initialized)
    {
        if (!BypassPCBootstrapper) return base.Launch(initialized);

        /*
            - Try to locate an already running instance of the game.
            - Try locating the game's window then the game's process.
        */

        if (Window is { } window) { window.SwitchToThisWindow(); return window.ProcessId; }
        if (Process is { } process1) { process1.WaitForInputIdle(); return process1.ProcessId; }

        /*
            - Bypass the PC Bootstrapper by using PowerShell & 'Invoke-CommandInDesktopPackage'.
            - We directly invoke 'Minecraft.Windows.exe' to bypass encryption imposed the GDK.
        */

        using (var @_ = PowerShell.Create())
        {
            @_.AddCommand("Invoke-CommandInDesktopPackage");

            @_.AddParameter("AppId", "Game");
            @_.AddParameter("PackageFamilyName", "Microsoft.MinecraftUWP_8wekyb3d8bbwe");
            @_.AddParameter("Command", Path.Combine(Package.InstalledPath, "Minecraft.Windows.exe"));

            @_.Invoke();
        }

        if (Process is not { } process2) return null;
        using (process2) { process2.WaitForInputIdle(); return process2.ProcessId; }
    }
}