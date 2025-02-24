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
using Flarial.Launcher.SDK;
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
     
        public static int progressPercentage;
        public static bool isDllDownloadFinished = false;
        public static bool isDownloadingVersion = false;
        public static long progressBytesReceived;
        public static long progressBytesTotal;
        public static string progressType; 
        public static bool shouldUseBetaDLL;
        public static ImageBrush PFP;
        public static bool Reverse;
        public static TextBlock StatusLabel;
        public static TextBlock versionLabel;
        public static TextBlock Username;
        private static StackPanel mbGrid;
        private static Stopwatch speed = new Stopwatch();
        public static volatile Action actionOnInject;
        public static UnhandledExceptionEventHandler unhandledExceptionHandler = (sender, args) =>
        {
            Exception ex = (Exception)args.ExceptionObject;
            string errorMessage = $"Unhandled exception: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

            Trace.WriteLine(errorMessage);
            try
            {
                System.Windows.MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                Trace.WriteLine("Failed to show error in MessageBox.");
            }
        };

        
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


            string today = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string filePath = $"{VersionManagement.launcherPath}\\{today}.txt";

            var outResultsFile = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read
            );

                var textListener = new AutoFlushTextWriterTraceListener(outResultsFile);
                Trace.Listeners.Add(textListener);
            
                Trace.WriteLine("Debug 0.5 " + stopwatch.Elapsed.Milliseconds.ToString());



           AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;
                
            Trace.WriteLine("Debug 1 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync(async () =>
            {
                await Config.loadConfig();

            });
            
            Trace.WriteLine("Debug 2 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync(Minecraft.Init);
            Trace.WriteLine("Debug 3 " + stopwatch.Elapsed.Milliseconds.ToString());
            
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
            
            Trace.WriteLine("Debug 7 " + stopwatch.Elapsed.Milliseconds.ToString());
            
            Trace.WriteLine("Debug 8 " + stopwatch.Elapsed.Milliseconds.ToString());

            Dispatcher.InvokeAsync(async () => await RPCManager.Initialize());

            Trace.WriteLine("Debug 9 " + stopwatch.Elapsed.Milliseconds.ToString());
            Application.Current.MainWindow = this;

            SetGreetingLabel();

            Dispatcher.InvokeAsync(async () => await TryDaVersionz());
            Trace.WriteLine("Debug 10 " + stopwatch.Elapsed.Milliseconds.ToString());
            
            stopwatch.Stop();
            
            MainWindow.CreateMessageBox("Join our discord! https://flarial.xyz/discord");

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
                AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
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
                AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
                Trace.Close();
                Environment.Exit(0);
            }
            else
            {
                CreateMessageBox("Flarial is currently downloading a version. You cannot close.");

            }
        }
        
        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
            bool compatible = await (await Catalog.GetAsync()).CompatibleAsync();
            if(!compatible) Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Flarial does not support this version of Minecraft.");
                if(Config.UseCustomDLL) MainWindow.CreateMessageBox("Custom DLL will be used.");
                
            });
            
            
            if(!Config.UseCustomDLL)
            {
                if(compatible)
                {
                    await SDK.Client.DownloadAsync(Config.UseBetaDLL, (value) => DownloadProgressCallback(value));
                    await SDK.Client.LaunchAsync(Config.UseBetaDLL);
                    StatusLabel.Text = "Launched! Enjoy.";
                }
            }
            else
            {
                StatusLabel.Text = "Launched Custom DLL! Enjoy.";
                await SDK.Minecraft.LaunchAsync(Config.CustomDLLPath);
            }
        }
        


        public void DownloadProgressCallback(int value)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = $"Downloaded {value}% of client";
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            
            Trace.Close();
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
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

public class AutoFlushTextWriterTraceListener : TextWriterTraceListener
{
    public AutoFlushTextWriterTraceListener(Stream stream) : base(stream) { }

    public override async void Write(string message)
    {
        await Writer.WriteAsync(message).ConfigureAwait(false);
        Writer.Flush();
    }

    public override async void WriteLine(string message)
    {
        await Writer.WriteLineAsync(message).ConfigureAwait(false);
        Writer.Flush();
    }
}


public class FileTraceListener : TraceListener
{
    private readonly StreamWriter _writer;

    public FileTraceListener(string filePath)
    {
        _writer = new StreamWriter(filePath, true);
        _writer.AutoFlush = true; // Enable AutoFlush if needed
    }

    public override async void Write(string message)
    {
        await _writer.WriteAsync(message).ConfigureAwait(false);
    }

    public override async void WriteLine(string message)
    {
        await _writer.WriteLineAsync(message).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Flush();
            _writer?.Close();
            _writer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
