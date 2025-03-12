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
        public static bool IsAdministrator =>
   new WindowsPrincipal(WindowsIdentity.GetCurrent())
       .IsInRole(WindowsBuiltInRole.Administrator);
    }
}
