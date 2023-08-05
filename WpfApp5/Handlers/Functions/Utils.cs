using Flarial.Launcher.Managers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Windows.Graphics.Capture;

namespace Flarial.Launcher.Functions
{
    public class Utils
    {
        public static bool IsGameOpen()
        {
            Process[] mc = Process.GetProcessesByName("Minecraft.Windows");
            return mc.Length > 0;
        }

        public static void disableVsync()
        {
            var mcPath = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                   "Packages",
                   "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                   "LocalState",
                   "games",
                   "com.mojang"
               );

            string path = Path.Combine(mcPath, "minecraftpe", "options.txt");

            if (File.Exists(path))
            {
            string options =    File.ReadAllText(path);
             string vsyncdisbaled =
                     options.Replace("gfx_vsync:1", "gfx_vsync:0");
                File.WriteAllText(path,vsyncdisbaled);

            }

      
        }
        public static void OpenGame()
        {
            if (IsGameOpen()) { return; }
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "explorer.exe",
                Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App",
            };
            process.StartInfo = startInfo;
            process.Start();
        }

  public      static bool IsDeveloperModeEnabled()
        {
            const string developerModeKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock";
            const string developerModeValueName = "AllowDevelopmentWithoutDevLicense";

            int value = (int)Registry.GetValue(developerModeKey, developerModeValueName, -1);
            return value == 1;
        }
        public static bool IsAdministrator =>
   new WindowsPrincipal(WindowsIdentity.GetCurrent())
       .IsInRole(WindowsBuiltInRole.Administrator);
    }
}
