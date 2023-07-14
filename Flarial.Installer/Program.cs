using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using IWshRuntimeLibrary;
using System.Reflection;
using System.Security.Policy;
using System.ComponentModel;
using System.Windows;

namespace Flarial.Minimal
{
    class Program
    {
        static private string location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Flarial\";
        static private string url = "https://cdn.flarial.net/launcher/latest.zip";

        static private void CreateShortcut(string name, string directory, string targetFile, string iconlocation, string description)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(directory + $@"\{name}.lnk");
            shortcut.TargetPath = targetFile;
            shortcut.IconLocation = iconlocation;
            shortcut.Description = description;
            shortcut.Save();
        }
        
        static private async void Install()
        {
            try
            {
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                WebClient client = new WebClient();

                bool done = false;

                client.DownloadFileCompleted += (object s, AsyncCompletedEventArgs e) =>
                {

                    
                    ZipFile.ExtractToDirectory(location + "latest.zip", location);

                    System.IO.File.Delete(location + "latest.zip");

                    CreateShortcut("Flarial", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), location + "flarial.launcher.exe", location + "flarial.launcher.exe", "Launch Flarial");
                    CreateShortcut("Flarial", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), location + "flarial.launcher.exe", location + "flarial.launcher.exe", "Launch Flarial");
                    CreateShortcut("Flarial Minimal", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), location + "flarial.minimal.exe", location + "flarial.minimal.exe", "Launch Flarial Minimal");
                    CreateShortcut("Flarial Minimal", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), location + "flarial.minimal.exe", location + "\\flarial.minimal.exe", "Launch Flarial Minimal");
                    done = true;
                };

                client.DownloadFileAsync(new Uri(url), location + "latest.zip");

                while (!done)
                {
                    
                }

                MessageBox.Show("Flarial has been installed.\nYou can find it on your desktop and in the windows menu.", "Flarial installer");
            } catch (Exception e)
            {
                MessageBox.Show($"Whoops! An error occurred: {e.Message}.\nPlease check your internet connection and if this keeps occurring contact us in our discord server.", "Flarial Installer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static void Main(string[] args)
        {
            if (Directory.Exists(location))
                foreach (string file in Directory.GetFiles(location))
                    System.IO.File.Delete(file);

            if (args.Length > 0) //custom installation path
            { 
                string newPath = "";

                foreach (string arg in args)
                    newPath += arg;
                
                if (Directory.Exists(newPath))
                    location = newPath;      
            }

            Install();
        }
    }
}
