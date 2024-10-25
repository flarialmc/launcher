using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Application = System.Windows.Application;
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
        public static bool isDownloadingVersion = false;
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
        public static volatile Action actionOnInject;

        
        private const int WM_CLOSE = 0x0010;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            };
            CreateDirectoriesAndFiles();
            
            Stopwatch stopwatch = new Stopwatch();
            speed.Start();
            stopwatch.Start();
            
            Trace.WriteLine("Debug 0 " + stopwatch.Elapsed.Milliseconds.ToString());

            TraceListenerCollection listeners = Trace.Listeners;

        

                string today = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-");
                FileStream outResultsFile = new FileStream(
                    $"{VersionManagement.launcherPath}\\{today}.txt",
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read
                );

                var textListener = new AutoFlushTextWriterTraceListener(outResultsFile);
                listeners.Add(textListener);
            
                Trace.WriteLine("\nMC Bought Status: " + Minecraft.StoreHelper.HasBought().Result);
                Trace.WriteLine("Debug 0.5 " + stopwatch.Elapsed.Milliseconds.ToString());
                
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                Trace.WriteLine($"Unhandled exception: {ex.Message}");
                Trace.WriteLine($"Stack Trace: {ex.StackTrace}");
            };
            
            Trace.WriteLine("Debug 1 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync((Action)(() => DownloadUtil()));

            Trace.WriteLine("Debug 2 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync((Action)(() => Minecraft.Init()));
            Trace.WriteLine("Debug 3 " + stopwatch.Elapsed.Milliseconds.ToString());

            Trace.WriteLine("Adding MC Load Loop");
            Task.Run(async () => await Minecraft.MCLoadLoop());
            Trace.WriteLine("Added!");

            Task.Run(() =>
            {
                Config.loadConfig();

            });

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
            updat.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/latestVersion.txt"), "latestVersion.txt");

            Trace.WriteLine("Debug 6");
            string[] updatRial = File.ReadAllLines("latestVersion.txt");

            CultureInfo germanCulture = new CultureInfo("de-DE");
            if (double.Parse(updatRial.First(), germanCulture) > version)
            {
                Trace.WriteLine("I try 2 autoupdate");

                updat.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/flarialmc/newcdn/main/installer.exe"), "installer.exe");

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
            
            MainWindow.CreateMessageBox("Join our discord! https://flarial.xyz/discord");

        }

        public async Task DownloadUtil()
        {
            string url = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/DllUtil.dll";
            string filePath = "dont.delete";
            
            WebClient updat = new WebClient();
            updat.DownloadFileAsync(new Uri(url), "dont.delete");
            
            filePath = "WUTokenHelper.dll";
            url = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/WUTokenHelper.dll";

        }

        public async Task<bool> TryDaVersionz()
        {
            
            VersionLabel.Text = Minecraft.GetVersion().ToString();


            WebClient versionsWc = new WebClient();
            versionsWc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/Supported.txt"), "Supported.txt");
            
            string[] rawVersions = await File.ReadAllLinesAsync("Supported.txt");
            string first = "Not Downloaded";

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
        private void Close(object sender, RoutedEventArgs e)
        {
            if(!isDownloadingVersion)
            {
                Trace.Close();
                this.Close();
            }
            else
            {
                CreateMessageBox("Flarial is currently downloading a version. You cannot close.");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLOSE)
            {
                if (isDownloadingVersion)
                {
                    CreateMessageBox("Flarial is currently downloading a version. You cannot close.");
                    handled = true;
                    return IntPtr.Zero;
                }
            }

            return IntPtr.Zero;
        }
        
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
            if(!isDownloadingVersion)
            {
                Trace.Close();
                Environment.Exit(0);
            }
            else
            {
                CreateMessageBox("Flarial is currently downloading a version. You cannot close.");

            }
        }
        
        public static void DownloadProgressCallbackOfDLL(object sender, DownloadProgressChangedEventArgs e)
        {
            isDllDownloadFinished = e.BytesReceived.ToString() == e.TotalBytesToReceive.ToString();
            
            if(isDllDownloadFinished)
            {
                Trace.WriteLine(isDllDownloadFinished);
                Trace.WriteLine("DLL Download Finished!");
            } else StatusLabel.Text = "DOWNLOADING DLL! " + Decimal.Round(e.BytesReceived, 2) + "/" + Decimal.Round(e.TotalBytesToReceive, 2);

            if (isDllDownloadFinished)
            {
                Trace.WriteLine("Download UPDATE: " + Minecraft.modules);
                Task.Run(async () => await Minecraft.RealDLLLoop());
            }
        }

        private async void Inject_Click(object sender, RoutedEventArgs e)
        {

            if (Minecraft.GetVersion().ToString() == "0.0.0")
            {
                CreateMessageBox("Minecraft is not installed.");
                return;
            }
            
            if (Config.UseCustomDLL)
            {
                Utils.OpenGame();

                Task.Run(async () =>
                {
                    Trace.WriteLine("Starting loop!");
                    await Minecraft.CustomDLLLoop();
                });

                return;
            }
            
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

                    string url = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/latest.dll";

                    WebClient updat = new WebClient();
                    updat.DownloadFileAsync(new Uri(url), filePath);
                    updat.DownloadProgressChanged += DownloadProgressCallbackOfDLL;
                }
                
                Trace.WriteLine("SPEED RN " + watch.Elapsed.Milliseconds);

                if (Config.UseCustomDLL) pathToExecute = Config.CustomDLLPath;
                
                Utils.OpenGame();

                actionOnInject = () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Trace.WriteLine("Injected!");
                        StatusLabel.Text = Insertion.Insert(pathToExecute).ToString();

                        if (!File.Exists("dont.delete") || !File.Exists("real.dll")) StatusLabel.Text = "Your antivirus has removed an important file.";
                    });
                };
                
                Trace.WriteLine("SPEED RN " + watch.Elapsed.Milliseconds);
            }
            else
                {
                    Trace.Write($"Not SUPPORTED {Minecraft.GetVersion()}");
                CreateMessageBox($"Our client does not support this MINECRAFT version {Minecraft.GetVersion()}. If you are using a custom dll, That will be used instead.");
                    if (Config.UseCustomDLL)
                    {
                       Utils.OpenGame();

                       Dispatcher.InvokeAsync(() =>
                       {
                           Trace.WriteLine("Starting loop!");

                           while (true)
                           {
                               if (Minecraft.modules >= 150)
                               {
                                   Trace.WriteLine("Injected!");
                                   Insertion.Insert(Config.CustomDLLPath);
                                   return;
                               }
                           }
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

static class WUTokenCaller
{
    [DllImport("WUTokenHelper.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int GetWUToken([MarshalAs(UnmanagedType.LPWStr)] out string token);
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
        DllReturns real = (DllReturns)DLLImports.AddTheDLLToTheGame(path);
        Trace.WriteLine(real.ToString());
        return real;
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

public class FileTraceListener : TraceListener
{
    private readonly StreamWriter _writer;

    public FileTraceListener(string filePath)
    {
        _writer = new StreamWriter(filePath, true);
    }

    public override void Write(string message)
    {
        _writer.Write(message);
    }

    public override void WriteLine(string message)
    {
        _writer.WriteLine(message);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Close();
            _writer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
