using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Diagnostics;
using System.Net;
using IWshRuntimeLibrary;
using System.Windows.Forms;
using Flarial.Installer;

namespace Flarial.Minimal
{
    class Program
    {
        static private ProgressBar bar;
        static private Progressbar form;
        static private string location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Flarial\";
        static private string url = "https://cdn-c6f.pages.dev/launcher/latest.zip";
        static private bool silent = false;

        static private void CreateShortcut(string name, string directory, string targetFile, string iconlocation, string description)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(directory + $@"\{name}.lnk");
            shortcut.TargetPath = targetFile;
            shortcut.IconLocation = iconlocation;
            shortcut.Description = description;
            shortcut.Save();
        }

        static private void Install()
        {
            Thread.Sleep(2000);
            if(Directory.Exists(location))
            Directory.Delete(location, true);
            try
            {
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                WebClient client = new WebClient();

                client.DownloadProgressChanged += (s, e) =>
                {

                    bar.Value = 2 + (int)(e.ProgressPercentage * 0.88);
                };

                client.DownloadFileCompleted += (s, e) =>
                {
                    
                    bar.Value = 92;
                    ZipFile.ExtractToDirectory(location + "latest.zip", location);

                    System.IO.File.Delete(location + "latest.zip");

                    Thread.Sleep(1000);
                    bar.Value = 96;
                    CreateShortcut("Flarial", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), location + "Flarial.Launcher.exe", location + "Flarial.Launcher.exe", "Launch Flarial");
                    CreateShortcut("Flarial", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), location + "Flarial.Launcher.exe", location + "Flarial.Launcher.exe", "Launch Flarial");

                    bar.Value = 98;
                    CreateShortcut("Flarial Minimal", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), location + "Flarial.Minimal.exe", location + "Flarial.Minimal.exe", "Launch Flarial Minimal");
                    CreateShortcut("Flarial Minimal", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), location + "Flarial.Minimal.exe", location + "Flarial.Minimal.exe", "Launch Flarial Minimal");

                    bar.Value = 100;

                    bar.Dispose();
                    

                    if (!silent)
                        MessageBox.Show("Flarial has been installed.\nYou can find it on your desktop and in the windows menu. WINDOWS DEFENDER MIGHT REMOVE FLARIAL BECAUSE IT'S DUMB! BE CAREFUL!", "Flarial Installer");

                    if (!silent)
                        form.Close();

                    Process.GetCurrentProcess().Kill();
                };

                client.DownloadFileAsync(new Uri(url), location + "latest.zip");

                Application.Run(form);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Whoops! An error occurred: {e.Message}.\nPlease check your internet connection and if this keeps occurring contact us in our discord server.", "Flarial Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0) //custom installation path
            {
                if (args[0] == "update")
                {
                    silent = true;
                    args[0] = "";
                    if (args.Length == 1)
                        goto nvm;
                }

                string newPath = "";

                foreach (string arg in args)
                    newPath += arg;

                Directory.CreateDirectory(newPath);

                location = newPath;
            }

            if (Directory.Exists(location))
                foreach (string file in Directory.GetFiles(location))
                    System.IO.File.Delete(file);

            nvm:

            form = new Progressbar();

            if (!silent)
                form.Show();
            else
                form.label1.Text = "Flarial Updater";

            bar = form.GetProgressBar();
            bar.Value = 2;


            Install();
        }
    }
}
