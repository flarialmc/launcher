using System;
using System.Diagnostics;
using System.IO;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Unmanaged;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe sealed class MinecraftGDK : Minecraft
{
    const string Command = $"Invoke-CommandInDesktopPackage -PackageFamilyName '{PackageFamilyName}' -AppId 'Game' -Command '{{0}}'";

    protected override string WindowClass => "Bedrock";
    protected override string ProcessName => "Minecraft.Windows.exe";

    static readonly string s_path, s_filename;

    static MinecraftGDK()
    {
        var system = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        s_path = Path.Combine(appdata, @"Minecraft Bedrock\Users");
        s_filename = Path.Combine(system, @"WindowsPowerShell\v1.0\powershell.exe");
    }

    protected override uint? Activate()
    {
        /*
            - We use PowerShell to directly start the game.
            - This simplifies the activation contract.
        */

        if (GetProcessId() is { } processId)
            return processId;

        var path = Path.Combine(Package.InstalledPath, ProcessName);
        if (!File.Exists(path)) throw new MinecraftNotFoundException();

        using var process = Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            FileName = s_filename,
            ArgumentList = { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", string.Format(Command, path) }
        });

        process?.WaitForExit();
        return GetProcessId();
    }

    internal override uint? Launch()
    {
        /*
            - Unlike UWP builds, we are bootstrapping the game manually.
            - Hence, verify if the game & Gaming Services are installed.
        */

        if (!IsInstalled)
            throw new MinecraftNotInstalledException();

        if (!IsGamingServicesInstalled)
            throw new GamingServicesNotInstalledException();

        if (GetWindow() is { } foundWindow && foundWindow.IsVisible)
        {
            foundWindow.Switch();
            return foundWindow._processId;
        }

        if (Activate() is not { } processId)
            return null;

        if (NativeProcess.Open(PROCESS_SYNCHRONIZE, processId) is not { } process)
            return null;

        using (process)
        {

            /*
                - Unpackaged installs might fail the launch contract.
                - Instead, wait for the game's window to be visible.
            */

            if (!IsPackaged)
            {
                NativeWindow? processWindow = null;

                while (process.Wait(1))
                {
                    processWindow = GetWindow(processId);
                    if (processWindow is { }) break;
                }

                while (process.Wait(1))
                {
                    var visible = processWindow?.IsVisible;
                    if (visible ?? false) return processId;
                }

                return null;
            }

            /*
                - The initialization logic is derived from the UWP builds of the game.
                - We don't need to resort to polling since symbolic links aren't used.
            */

            var handle = CreateEvent(null, true, false, null);
            try
            {
                using FileSystemWatcher watcher = new(Directory.CreateDirectory(s_path).FullName, "*menu_load_lock")
                {
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                };

                watcher.Deleted += (_, _) => SetEvent(handle);
                var handles = stackalloc HANDLE[] { handle, process };

                if (WaitForMultipleObjects(2, handles, false, INFINITE) is WAIT_OBJECT_0)
                    return processId;

                return null;
            }
            finally { CloseHandle(handle); }
        }
    }
}