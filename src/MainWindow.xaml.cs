using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Animations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
using File = System.IO.File;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Bedrockix.Windows;
using Bedrockix.Minecraft;
using Windows.ApplicationModel;

namespace Flarial.Launcher;

public partial class MainWindow
{
    [DllImport("Shell32", PreserveSig = true, ExactSpelling = true, CharSet = CharSet.Unicode, EntryPoint = "ShellExecuteW")]
    static extern nint ShellExecute(nint hwnd = default, string lpOperation = default, string lpFile = default, string lpParameters = default, string lpDirectory = default, int nShowCmd = default);

    public int version = 200;

    public static int progressPercentage;

    public static bool isDownloadingVersion = false;

    public static long progressBytesReceived;

    public static long progressBytesTotal;

    public static string progressType;

    public static bool isPremium;

    public static bool isLoggedIn;

    public static ImageBrush PFP;

    public static bool Reverse;

    public static TextBlock StatusLabel;

    public static TextBlock versionLabel;

    public static TextBlock Username;

    private static StackPanel mbGrid;

    private static readonly Stopwatch speed = new();

    readonly WindowInteropHelper WindowInteropHelper;

    public static SDK.Catalog VersionCatalog;

    public bool IsLaunchEnabled
    {
        get { return (bool)GetValue(IsLaunchEnabledProperty); }
        set { SetValue(IsLaunchEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsLaunchEnabledProperty =
        DependencyProperty.Register("IsLaunchEnabled", typeof(bool),
            typeof(MainWindow), new PropertyMetadata(true));

    public bool updateTextEnabled
    {
        get { return (bool)GetValue(updateTextEnabledProperty); }
        set { SetValue(updateTextEnabledProperty, value); }
    }

    public static readonly DependencyProperty updateTextEnabledProperty =
        DependencyProperty.Register("updateTextEnabled", typeof(bool),
            typeof(MainWindow), new PropertyMetadata(true));

    public int updateProgress
    {
        get { return (int)GetValue(updateProgressProperty); }
        set { SetValue(updateProgressProperty, value); }
    }

    public static readonly DependencyProperty updateProgressProperty =
        DependencyProperty.Register("updateProgress", typeof(int),
            typeof(MainWindow), new PropertyMetadata(0));


    public static UnhandledExceptionEventHandler unhandledExceptionHandler = (sender, args) =>
    {
        Exception ex = (Exception)args.ExceptionObject;
        string errorMessage = $"Version: {Assembly.GetEntryAssembly().GetName().Version}\nUnhandled exception: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

        Trace.WriteLine(errorMessage);
        try
        {
            MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            Trace.WriteLine("Failed to show error in MessageBox.");
        }
        Environment.Exit(default);
    };

    // Parralax background image
    private const double Smoothness = 0.1;
    private const double Strength = 0.04;
    private double _targetX = 0;
    private double _targetY = 0;

    private void OnRenderFrame(object sender, EventArgs e)
    {
        if (!Config.BackgroundParallaxEffect) return;

        CursorFollowTransform.X += (_targetX - CursorFollowTransform.X) * Smoothness;
        CursorFollowTransform.Y += (_targetY - CursorFollowTransform.Y) * Smoothness;
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!Config.BackgroundParallaxEffect) return;

        var position = e.GetPosition(this);
        _targetX = (position.X - BackgroundImage.ActualWidth / 2) * Strength;
        _targetY = (position.Y - BackgroundImage.ActualHeight / 2) * Strength;
    }

    public void CenterParrallaxImage()
    {
        _targetX = 0;
        _targetY = 0;
        CursorFollowTransform.X = 0;
        CursorFollowTransform.Y = 0;
    }

    public MainWindow()
    {
        Config.LoadConfig();

        InitializeComponent();
        CompositionTarget.Rendering += OnRenderFrame;

        WindowInteropHelper = new(this);

        using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("app.ico"))
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SnapsToDevicePixels = UseLayoutRounding = true;

        MouseLeftButtonDown += (_, _) => { try { DragMove(); } catch { } };
        ContentRendered += MainWindow_ContentRendered;

        CreateDirectoriesAndFiles();

        Stopwatch stopwatch = new();
        speed.Start();
        stopwatch.Start();

        Trace.WriteLine("Debug 0 " + stopwatch.Elapsed.Milliseconds.ToString());

        Trace.WriteLine("Debug 0.5 " + stopwatch.Elapsed.Milliseconds.ToString());

        AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;

        Trace.WriteLine("Debug 1 " + stopwatch.Elapsed.Milliseconds.ToString());

        LauncherVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version;

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

        if (Config.Rpc)
        {
            RPCManager.InitializeRPC();
        }

        Trace.WriteLine("Debug 9 " + stopwatch.Elapsed.Milliseconds.ToString());
        Application.Current.MainWindow = this;

        SetGreetingLabel();
        Trace.WriteLine("Debug 10 " + stopwatch.Elapsed.Milliseconds.ToString());

        stopwatch.Stop();

        if (Config.WelcomeMessage)
        {
            CreateMessageBox("Join our discord! https://flarial.xyz/discord");
        }

        IsLaunchEnabled = false;

        StartRefreshTimer();

        Catalog.PackageInstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, true); };
        Catalog.PackageUpdating += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.TargetPackage, true); };
        Catalog.PackageUninstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, false); };
    }

    private System.Timers.Timer refreshTimer;

    private void StartRefreshTimer()
    {
        refreshTimer = new System.Timers.Timer(1.5 * 60 * 1000);
        refreshTimer.Elapsed += (sender, e) => RefreshWebView();
        refreshTimer.AutoReset = true;
        refreshTimer.Enabled = true;
    }

    private void RefreshWebView()
    {
    }

    readonly PackageCatalog Catalog = PackageCatalog.OpenForCurrentUser();

    async Task UpdateVersionLabel(Package source, bool value)
    {
        if (source.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", StringComparison.OrdinalIgnoreCase))
            await Task.Run(() =>
            {
                var text = value ? SDK.Minecraft.Version : "0.0.0";
                Dispatcher.Invoke(() => VersionLabel.Text = text);
            });
    }

    protected override async void OnInitialized(EventArgs e)
    {
        await Task.Run(Config.LoadConfig);
        if (Game.Installed) await Task.Run(() => { var text = SDK.Minecraft.Version; Dispatcher.Invoke(() => VersionLabel.Text = text); });
        base.OnInitialized(e);
    }

    private async void MainWindow_ContentRendered(object sender, EventArgs e)
    {
        if (await SDK.Launcher.AvailableAsync())
        {
            updateTextEnabled = true;

            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = false;
                MainGrid.Visibility = Visibility.Hidden;
                mbGrid.Visibility = Visibility.Hidden;
                LolGrid.Visibility = Visibility.Visible;
                LolGrid.IsEnabled = true;
            }, DispatcherPriority.ApplicationIdle);

            await SDK.Launcher.UpdateAsync(DownloadProgressCallback2);
        }

        Catalog.PackageInstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, true); };
        Catalog.PackageUpdating += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.TargetPackage, true); };
        Catalog.PackageUninstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, false); };

        if (Game.Installed) await Task.Run(() => { var text = SDK.Minecraft.Version; Dispatcher.Invoke(() => VersionLabel.Text = text); });

        VersionCatalog = await SDK.Catalog.GetAsync();
        IsLaunchEnabled = HomePage.IsEnabled = true;
    }

    public static void CreateMessageBox(string text)
        => mbGrid.Children.Add(new Styles.MessageBox(text));

    public static void CreateMessageBox(string text, int lifetime)
        => mbGrid.Children.Add(new Styles.MessageBox(text, lifetime));

    public static void CreateMessageBox(string text, bool autoCalculateLifetime)
        => mbGrid.Children.Add(new Styles.MessageBox(text, autoCalculateLifetime: autoCalculateLifetime));

    private void MoveWindow(object sender, MouseButtonEventArgs e) => DragMove();
    private void Minimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close(object sender, RoutedEventArgs e)
    {
        if (!isDownloadingVersion)
        {
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
            Trace.Close();
            Close();
        }
        else
        {
            CreateMessageBox("Flarial is currently downloading a version. You cannot close.");
        }
    }



    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) =>
        SettingsPageTransition.SettingsEnterAnimation(MainBorder, MainGrid);

    private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        NewsPageTransition.Animation(Reverse, MainBorder, NewsBorder, NewsArrow);


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

        var info = Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Logs");
        Trace.Listeners.Add(new AutoFlushTextWriterTraceListener(File.Create($@"{info.FullName}\{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt")));
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

    private async void Inject_Click(object sender, RoutedEventArgs e)
    {
        IsLaunchEnabled = false;

        if (!SDK.Minecraft.Installed)
        {
            CreateMessageBox("Minecraft isn't installed, please install it!");
            IsLaunchEnabled = true; return;
        }

        if (await Task.Run(() => Metadata.Instancing))
        {
            CreateMessageBox("Flarial Client doesn't support multi-instancing, please disable it!");
            IsLaunchEnabled = true; return;
        }

        bool compatible = await VersionCatalog.CompatibleAsync();
        if (!Config.CustomDll && !compatible)
        {
            CreateMessageBox("Flarial does not support this version of Minecraft.");
            IsLaunchEnabled = true; return;
        }


        if (!Config.CustomDll)
        {
            if (compatible)
            {
                await SDK.Client.DownloadAsync(Config.BetaDll, DownloadProgressCallback);
                await SDK.Client.LaunchAsync(Config.BetaDll);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Text = "Launched! Enjoy.";
                    IsLaunchEnabled = true;
                });
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(Config.CustomDllPath))
            {
                Library value = new(Config.CustomDllPath);

                if (value.Valid)
                {
                    await Task.Run(() => Loader.Launch(value));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusLabel.Text = "Launched Custom DLL! Enjoy.";
                        IsLaunchEnabled = true;
                    });
                }
                else
                    CreateMessageBox("The specified Custom DLL is potentially invalid or doesn't exist.");
            }
            else CreateMessageBox("Please specify a Custom DLL.");
        }

        IsLaunchEnabled = true;
    }



    public void DownloadProgressCallback(int value)
    {

        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusLabel.Text = $"Downloaded {value}% of client";
        });
    }

    public void DownloadProgressCallback2(int value)
    {

        Application.Current.Dispatcher.Invoke(() =>
        {
            updateProgress = value;
        });
    }

    private async void SaveConfig(object sender, RoutedEventArgs e) => await Task.Run(() => Config.SaveConfig());

    private void Window_OnClosing(object sender, CancelEventArgs e)
    {
        Trace.Close();
        AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
        Environment.Exit(0);
    }

    private void AdBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
    ShellExecute(hwnd: WindowInteropHelper.EnsureHandle(), lpFile: "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch");
}

public class AutoFlushTextWriterTraceListener(FileStream stream) : TextWriterTraceListener(stream)
{
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
        _writer = new StreamWriter(filePath, true)
        {
            AutoFlush = true // Enable AutoFlush if needed
        };
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
