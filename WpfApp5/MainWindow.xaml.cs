using Flarial.Launcher.Functions;
using Flarial.Launcher.Handlers.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Animations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Flarial.Launcher
{
    public partial class MainWindow
    {
        private static readonly WebClient Client = new WebClient();
        List<double> data = new List<double>();
        public int version = 200; // 0.666 will be ignored by the updater, hence it wont update. But for release, it is recommended to use an actual release number.
        public static bool isLoggedIn;
        private Dictionary<string, string> TestVersions = new Dictionary<string, string>();
     
        // PLZ REMBER TO CHECK IF USER IS BETA TOO. DONT GO AROUND USING THIS OR ELS PEOPL CAN HAC BETTA DLL!!
        public static int progressPercentage;
        public static bool isDllDownloadFinished = false;
        public static long progressBytesReceived;
        public static long progressBytesTotal;
        public static string progressType; 
        public bool shouldUseBetaDLL;
        public static ImageBrush PFP;
        public static bool Reverse;
        public static TextBlock StatusLabel;
        public static TextBlock versionLabel;
        public static TextBlock Username;
        private static StackPanel mbGrid;
        private static Stopwatch speed = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            CreateDirectoriesAndFiles();
            
            
            Stopwatch stopwatch = new Stopwatch();
            speed.Start();
            stopwatch.Start();
            
            Trace.WriteLine("Debug 0 " + stopwatch.Elapsed.Milliseconds.ToString());

            FileStream outResultsFile = new FileStream(
                $"{VersionManagement.launcherPath}\\log.txt",
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read
            );

            var textListener = new AutoFlushTextWriterTraceListener(outResultsFile);
            Trace.Listeners.Add(textListener);
            
            Trace.WriteLine("Debug 1 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync((Action)(() => DownloadUtil()));

            Trace.WriteLine("Debug 2 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync((Action)(() => Minecraft.Init()));
            Trace.WriteLine("Debug 3 " + stopwatch.Elapsed.Milliseconds.ToString());



            Config.loadConfig();

            StatusLabel = statusLabel;
            versionLabel = VersionLabel;
            Username = username;
            mbGrid = MbGrid;
            PFP = pfp;
            SettingsPage.MainGrid = MainGrid;
            SettingsPage.b1 = MainBorder;

            Environment.CurrentDirectory = VersionManagement.launcherPath;

            Trace.WriteLine("Debug 4 " + stopwatch.Elapsed.Milliseconds.ToString());

            Trace.WriteLine("Debug 5 " + stopwatch.Elapsed.Milliseconds.ToString());


            if (version == 0.666)
            {
                Trace.WriteLine("It's development time.");
            }

            /*
            WebClient updat = new WebClient();
            updat.DownloadFileAsync(new Uri("https://flarialbackup.ashank.tech/launcher/latestVersion.txt"), "latestVersion.txt");

            Trace.WriteLine("Debug 6");
            string[] updatRial = File.ReadAllLines("latestVersion.txt");

            CultureInfo germanCulture = new CultureInfo("de-DE");
            if (double.Parse(updatRial.First(), germanCulture) > version)
            {
                Trace.WriteLine("I try 2 autoupdate");

                updat.DownloadFileAsync(new Uri("https://flarialbackup.ashank.tech/installer.exe"), "installer.exe");

                var p = new Process();
                p.StartInfo.FileName = "installer.exe";
                p.StartInfo.Arguments = "update";
                p.Start();

                Trace.Close();
                Environment.Exit(5);
            }
            */
            
            Trace.WriteLine("Debug 7 " + stopwatch.Elapsed.Milliseconds.ToString());
            
            Trace.WriteLine("Debug 8 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync((Action)(() => RPCManager.Initialize()));

            Trace.WriteLine("Debug 9 " + stopwatch.Elapsed.Milliseconds.ToString());
            Application.Current.MainWindow = this;

            SetGreetingLabel();

            Dispatcher.InvokeAsync((Action)(() => TryDaVersionz()));
            
            Trace.WriteLine("Debug 10 " + stopwatch.Elapsed.Milliseconds.ToString());
            
            stopwatch.Stop();

        }

        public async Task DownloadUtil()
        {
            string url = "https://flarialbackup.ashank.tech/dll/DllUtil.dll";
            string filePath = "dont.delete";
            
            WebClient updat = new WebClient();
            updat.DownloadFileAsync(new Uri(url), "dont.delete");
        }

        public async Task<bool> TryDaVersionz()
        {
            
            VersionLabel.Text = Minecraft.GetVersion().ToString();


            WebClient versionsWc = new WebClient();
            versionsWc.DownloadFileAsync(new Uri("https://flarialbackup.ashank.tech/launcher/Supported.txt"), "Supported.txt");
            
            string[] rawVersions = await File.ReadAllLinesAsync("Supported.txt");
            string first = "Not Installed";

            if (Minecraft.GetVersion().ToString().StartsWith("0"))
            {
                CreateMessageBox("You don't have Minecraft installed. Go to Options and try our Version Changer.");
            } else if (rawVersions.Contains(Minecraft.GetVersion().ToString()) == false)
            {
                CreateMessageBox("You are currently using a Minecraft version unsupported by Flarial");
                StatusLabel.Text = $"{Minecraft.GetVersion()} is unsupported";
            }

            return true;
        }

        public static void CreateMessageBox(string text)
        {
            mbGrid.Children.Add(new Flarial.Launcher.Styles.MessageBox { Text = text });
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void Minimize(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Close(object sender, RoutedEventArgs e) => this.Close();
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) =>
            SettingsPageTransition.SettingsEnterAnimation(MainBorder, MainGrid);
        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            NewsPageTransition.Animation(Reverse, MainBorder, NewsBorder, NewsArrow);

        private RadioButton CreateRadioButton(string background)
        {
            return new RadioButton { Tag = new ImageBrush { ImageSource = new ImageSourceConverter().ConvertFromString(background) as ImageSource } };
        }
        private void CreateDirectoriesAndFiles()
        {
            Trace.WriteLine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial"));
            
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial")))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial"));
            
            if (!Directory.Exists(VersionManagement.launcherPath))
                Directory.CreateDirectory(VersionManagement.launcherPath);
            
            if (!Directory.Exists(BackupManager.backupDirectory))
                Directory.CreateDirectory(BackupManager.backupDirectory);

            if (!Directory.Exists(VersionManagement.launcherPath + "\\Versions\\"))
                Directory.CreateDirectory(VersionManagement.launcherPath + "\\Versions\\");

            if (!File.Exists($"{VersionManagement.launcherPath}\\cachedToken.txt"))
                File.Create($"{VersionManagement.launcherPath}\\cachedToken.txt");
        }

        
        static private void CreateShortcut(string name, string directory, string targetFile, string iconlocation, string description)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(directory + $@"\{name}.lnk");
            shortcut.TargetPath = targetFile;
            shortcut.IconLocation = iconlocation;
            shortcut.Description = description;
            shortcut.Save();
        }

        private void SetGreetingLabel()
        {
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));

            if (Time >= 0 && Time < 12)
                GreetingLabel.Text = "Good Morning!";
            else if (Time >= 12 && Time < 18)
                GreetingLabel.Text = "Good Afternoon!";
            else if (Time >= 18 && Time <= 24)
                GreetingLabel.Text = "Good Evening!";
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Trace.Close();
            Environment.Exit(0);
        }
        
        public static void DownloadProgressCallbackOfDLL(object sender, DownloadProgressChangedEventArgs e)
        {
            isDllDownloadFinished = (e.BytesReceived == e.TotalBytesToReceive);
            StatusLabel.Text = "DOWNLOADING DLL! " + Decimal.Round(e.BytesReceived, 2) + "/" + Decimal.Round(e.TotalBytesToReceive, 2);
        }

        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(speed.Elapsed.Milliseconds);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Trace.WriteLine("SPEED RN " + watch.Elapsed.Milliseconds);
            
            Trace.WriteLine("did work");
            
                if (File.ReadAllText("Supported.txt").Contains(Minecraft.GetVersion().ToString()))
                {
                    
                
                string filePath = Path.Combine(VersionManagement.launcherPath, "real.dll");
                string pathToExecute = filePath;
                
                if(!Config.UseCustomDLL)
                {
                    isDllDownloadFinished = false;
                    StatusLabel.Text = "DOWNLOADING DLL! This may take some time depending on your internet.";

                    string url = "https://flarialbackup.ashank.tech/dll/latest.dll";

                    WebClient updat = new WebClient();
                    updat.DownloadFileAsync(new Uri(url), filePath);
                    updat.DownloadProgressChanged += DownloadProgressCallbackOfDLL;
                }
                
                Trace.WriteLine("SPEED RN " + watch.Elapsed.Milliseconds);

                if (Config.UseCustomDLL) pathToExecute = Config.CustomDLLPath;
                
                Utils.OpenGame();

                Action action = () =>
                {
                    StatusLabel.Text = Insertion.Insert(pathToExecute).ToString();
                };

                Task.Run(() =>
                {
                    Minecraft.WaitForModules(action);
                });
                
                Trace.WriteLine("SPEED RN " + watch.Elapsed.Milliseconds);
            }
            else
                {
                CreateMessageBox("Our client does not support this version. If you are using a custom dll, That will be used instead.");
                    if (Config.UseCustomDLL)
                    {
                       Utils.OpenGame();

                       isDllDownloadFinished = true;

                       Action action = () =>
                       {
                           Insertion.Insert(Config.CustomDLLPath);
                       };

                       Task.Run(() =>
                       {
                           Minecraft.WaitForModules(action);
                       });
                    }
                }
        }


        public void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)

        {

            // Displays the operation identifier, and the transfer progress.

            StatusLabel.Text = $" Downloaded {e.ProgressPercentage}% of client";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.Close();
            Environment.Exit(0);
        }


        private async void SaveConfig(object sender, RoutedEventArgs e)
        {
            await Config.saveConfig();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

    }
}

static class DLLImports
{

    [DllImport("dont.delete", CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddTheDLLToTheGame(string path);
}

public enum DllReturns
{
    SUCCESS = 0,
    ERROR_PROCESS_NOT_FOUND = 1,
    ERROR_PROCESS_OPEN = 2,
    ERROR_ALLOCATE_MEMORY = 3,
    ERROR_WRITE_MEMORY = 4,
    ERROR_GET_PROC_ADDRESS = 5,
    ERROR_CREATE_REMOTE_THREAD = 6,
    ERROR_WAIT_FOR_SINGLE_OBJECT = 7,
    ERROR_VIRTUAL_FREE_EX = 8,
    ERROR_CLOSE_HANDLE = 9,
    ERROR_UNKNOWN = 10,
    ERROR_NO_PATH = 11,
    ERROR_NO_ACCESS = 12,
    ERROR_NO_FILE = 13
}
public class Insertion
{
    public static DllReturns Insert(string path)
    {
        return (DllReturns)DLLImports.AddTheDLLToTheGame(path);
    }
}

public class AutoFlushTextWriterTraceListener : TextWriterTraceListener
{
    public AutoFlushTextWriterTraceListener(Stream stream) : base(stream) { }

    public override void Write(string message)
    {
        base.Write(message);
        Flush();
    }

    public override void WriteLine(string message)
    {
        base.WriteLine(message);
        Flush();
    }
}
