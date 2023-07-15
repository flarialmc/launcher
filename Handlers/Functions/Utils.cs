using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Flarial.Launcher.Functions
{


    public static class UserUtils
    {
        // Import the necessary Windows API functions
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertStringSidToSid(string stringSid, out IntPtr sid);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        public static string GetCurrentUserId()
        {
            try
            {
                // Get the current Windows identity
                var windowsIdentity = WindowsIdentity.GetCurrent();

                // Convert the user's SID to string format
                string stringSid = windowsIdentity.User.Value;

                // Convert the string SID to a valid SID
                if (ConvertStringSidToSid(stringSid, out IntPtr sidPtr))
                {
                    try
                    {
                        // Convert the SID to string format
                        string sidString = new SecurityIdentifier(sidPtr).ToString();

                        // Return the user's SID string
                        return sidString;
                    }
                    finally
                    {
                        // Free the memory allocated for the SID pointer
                        LocalFree(sidPtr);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the retrieval
                Console.WriteLine($"Error retrieving user SID: {ex.Message}");
            }

            return null;
        }
    }
    public class Utils
    {
        public static bool IsGameOpen()
        {
            Process[] mc = Process.GetProcessesByName("Minecraft.Windows");
            return mc.Length > 0;
        }

        public static void OpenGame()
        {
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

        public static bool IsAdministrator =>
   new WindowsPrincipal(WindowsIdentity.GetCurrent())
       .IsInRole(WindowsBuiltInRole.Administrator);
    }
}
