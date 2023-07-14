using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Flarial.Minimal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string dllpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Flarial.dll";

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
                client.DownloadFileAsync(new Uri("https://cdn.flarial.net/dll/latest.dll"), dllpath);
                bool done = false;
                client.DownloadFileCompleted += (object s, AsyncCompletedEventArgs e) =>
                {
                    done = true;
                    Console.WriteLine("Latest DLL has been downloaded");
                };

                while (!done) { }

                Console.WriteLine("Injecting");
                Console.WriteLine(Insertion.Insert(dllpath));
            }
            Console.WriteLine("Injected");
            Thread.Sleep(3000);
        }
    }
}
