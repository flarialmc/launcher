using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Flarial.Minimal
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            Minecraft.init();

            string dllpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Flarial.dll";

            if (Process.GetProcessesByName("Minecraft.Windows.exe").Length == 0)
            {
                Process procress = new Process();
                procress.StartInfo.Arguments = "shell:AppsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App";
                procress.StartInfo.FileName = "explorer.exe";
                procress.Start();

            check:
                int checks = 0;
                Process[] processes = Process.GetProcessesByName("Minecraft.Windows");
                if (checks > 100)
                {
                    Console.Write("Error: The game took too long to launch");
                    return;
                }
                if (processes.Length == 0)
                {
                    Thread.Sleep(100);
                    checks++;
                    goto check;
                }

            }

            if (args.Length > 0)
            {
                Console.WriteLine("Checking for custom path");
                string newPath = "";

                foreach (string arg in args)
                {
                    newPath += arg;
                }

                if (File.Exists(newPath))
                {
                    dllpath = newPath;
                    Console.WriteLine("Injecting custom dll");
                    Console.WriteLine(Insertion.Insert(dllpath));
                }
            }
            else
            {
                WebClient client = new WebClient();
                Console.WriteLine("Downloading the latest DLL");
                await client.DownloadFileTaskAsync(new Uri("https://cdn-c6f.pages.dev/dll/latest.dll"), dllpath);
           
                    Console.WriteLine("Latest DLL has been downloaded");
                    Console.WriteLine("Waiting for Minecraft to load.");
                    
                    await Minecraft.WaitForModules();
                    Console.WriteLine(Insertion.Insert(dllpath));
            }
            Console.WriteLine("Enjoy!");
            Thread.Sleep(3000);
        }
    }
}
