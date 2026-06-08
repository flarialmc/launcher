using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Unmanaged;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe partial class Minecraft
{
    static readonly string s_path, s_filename;

    static uint? Activate()
    {
        if (GetProcessId() is { } processId)
            return processId;

        var path = Path.Combine(Package.InstalledPath, ProcessName);
        if (!File.Exists(path)) throw new GameNotFoundException();

        using var process = Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = s_filename,
            ArgumentList = { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", string.Format(Command, path) }
        });

        process?.WaitForExit();
        return GetProcessId();
    }

    internal static uint? Launch()
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!IsInstalled)
            throw new GameNotFoundException();

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
                - Sideloaded installs might fail the launch contract.
                - Instead, wait for the game's window to be visible.
            */

            if (IsSideloaded)
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