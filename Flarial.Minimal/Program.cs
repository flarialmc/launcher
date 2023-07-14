using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel;
using Microsoft.SqlServer.Server;
using System.IO;

namespace Flarial.Minimal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string dllpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Flarial.dll";

            if (args.Length > 0)
            {
                string newPath = "";

                foreach (string arg in args)
                {
                    newPath += arg;
                }

                if (File.Exists(newPath))
                {
                    dllpath = newPath;
                }

                DLLImports.AddTheDLLToTheGame(dllpath);
            }
            else if (!System.IO.File.Exists(dllpath))
            {
                WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri("https://cdn.flarial.net/dll/flarial.dll"), "Flarial.dll");
                client.DownloadFileCompleted += (object s, AsyncCompletedEventArgs e) =>
                {
                    DLLImports.AddTheDLLToTheGame(dllpath);
                };
            }
                
        }
    }
}
