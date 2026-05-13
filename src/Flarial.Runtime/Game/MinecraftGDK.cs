using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Flarial.Runtime.Unmanaged;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Directory;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.Foundation.WIN32_ERROR;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe sealed class MinecraftGDK : Minecraft
{
    internal MinecraftGDK() : base() { }

    protected override string WindowClass => "Bedrock";
    protected override string ProcessName => "Minecraft.Windows.exe";

    static readonly string s_path = Path.Combine(GetFolderPath(ApplicationData), @"Minecraft Bedrock\Users");

    protected override uint? Activate()
    {
        /*
            - We use PowerShell to directly start the game.
            - This simplifies the activation contract.
        */

        if (GetProcessId() is { } processId)
            return processId;

        var path = Path.Combine(Package.InstalledPath, ProcessName);
        if (!File.Exists(path)) return null;

        var command = string.Join("; ",
            "Import-Module Appx",
            $"Invoke-CommandInDesktopPackage -AppId 'Game' -Command {Quote(path)} -PackageFamilyName {Quote(PackageFamilyName)}");

        using var powershell = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        });

        powershell?.WaitForExit();

        for (var attempt = 0; attempt < 50; attempt++)
        {
            if (GetProcessId() is { } launchedProcessId)
                return launchedProcessId;

            Thread.Sleep(100);
        }

        return null;
    }

    static string Quote(string value) => $"'{value.Replace("'", "''")}'";

    internal override uint? Launch()
    {
        /*
            - Unlike UWP builds, we are bootstrapping the game manually.
            - Hence, verify if the game & Gaming Services are installed.
        */

        if (!IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!IsGamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

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
                - Unsigned installs might fail the launch contract.
                - Instead, wait for the game's window to be visible.
            */

            if (!IsPackaged)
            {
                NativeWindow? processWindow = null;

                while (process.Wait(1))
                    if ((processWindow = GetWindow()) is { })
                        break;

                while (process.Wait(1))
                    if (processWindow?.IsVisible ?? false)
                        return processId;

                return null;
            }

            /*
                - The initialization logic is derived from the UWP builds of the game.
                - We don't need to resort to polling since symbolic links aren't used.
            */

            var handle = CreateEvent(null, true, false, null); try
            {
                using FileSystemWatcher watcher = new(CreateDirectory(s_path).FullName, "*menu_load_lock")
                {
                    InternalBufferSize = 0,
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
