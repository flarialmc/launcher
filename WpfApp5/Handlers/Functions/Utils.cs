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
