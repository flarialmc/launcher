using System.Diagnostics;

namespace Flarial.Launcher.Functions
{
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
    }
}
