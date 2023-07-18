using Flarial.Launcher.Functions;
using Flarial.Launcher.Handlers.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Flarial.NewsPanel;
using Newtonsoft.Json;
using ScrollAnimateBehavior.AttachedBehaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class News
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }
        public string AuthorAvatar { get; set; }
        public string Date { get; set; }
        public string Background { get; set; }
    }

    public class Root
    {
        public List<News> News { get; set; }
    }

    public partial class MainWindow
    {
        public int duration = 300;

        private static readonly WebClient Client = new WebClient();
        List<double> data = new List<double>();
        Root deserializedNews;

        Storyboard myWidthAnimatedButtonStoryboard1 = new Storyboard();
        Storyboard myWidthAnimatedButtonStoryboard2 = new Storyboard();
        Storyboard myWidthAnimatedButtonStoryboard3 = new Storyboard();
        public Window1 w = new Window1();
        public bool ifBeta;
        public double version = 1.2; // 0.666 will be ignored by the updater, hence it wont update. But for release, it is recommended to use an actual release number.
        public string minecraft_version = "amongus";
        public static string custom_dll_path = "amongus";
        public static string custom_theme_path = "main_default";
        public bool closeToTray;
        public bool autoLogin;
        public bool isLoggedIn;
        private Dictionary<string, string> TestVersions = new Dictionary<string, string>();
        private string ChosenVersion;
        // PLZ REMBER TO CHECK IF USER IS BETA TOO. DONT GO AROUND USING THIS OR ELS PEOPL CAN HAC BETTA DLL!!
        public bool shouldUseBetaDLL;
        private ImageSource guestImage;
        public static int progressPercentage;
        public static long progressBytesReceived;
        public static long progressBytesTotal;
        public static string progressType;

        public MainWindow()
        {

            loadConfig();

            if (!FontManager.IsFontInstalled("Unbounded"))
            {
                Client?.DownloadFile("https://cdn.flarial.net/assets/Unbounded-VariableFont_wght.ttf", "Unbounded-VariableFont_wght.ttf");
                FontManager.InstallFont($"{Directory.GetCurrentDirectory()}\\Unbounded-VariableFont_wght.ttf");
                File.Delete($"{Directory.GetCurrentDirectory()}\\Unbounded-VariableFont_wght.ttf");
            }

            if (!FontManager.IsFontInstalled("Sofia Sans"))
            {
                Client?.DownloadFile("https://cdn.flarial.net/assets/SofiaSans-VariableFont_wght.ttf", "SofiaSans-VariableFont_wght.ttf");
                FontManager.InstallFont($"{Directory.GetCurrentDirectory()}\\SofiaSans-VariableFont_wght.ttf");
                File.Delete($"{Directory.GetCurrentDirectory()}\\SofiaSans-VariableFont_wght.ttf");
            }
            
            //Trace.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

            if (!Utils.IsAdministrator)
            {
                MessageBox.Show("Run the application as an Administrator to continue.");
                Process.GetCurrentProcess().Kill();
            }

            Minecraft.Init();
            CreateDirectoriesAndFiles();

            Environment.CurrentDirectory = VersionManagement.launcherPath;

            InitializeComponent();

            if (autoLogin)
                AttemptLogin();

            if (custom_dll_path != "amongus")
            {
                CustomDllButton.IsChecked = true;
                dllTextBox.Visibility = Visibility.Visible;
                dllTextBox.Text = custom_dll_path;
            }


            TrayButton.IsChecked = closeToTray;

            if (!(custom_theme_path == "main_default"))
            {
                if (!string.IsNullOrEmpty(custom_theme_path))
                {
                    CustomThemeButton.IsChecked = true;
                    themeTextBox.Visibility = Visibility.Visible;
                    themeTextBox.Text = custom_theme_path;
                    var app = (App)Application.Current;
                    app.ChangeTheme(new Uri(custom_theme_path, UriKind.Absolute));
                }
            }

            BetaDLLButton.IsChecked = shouldUseBetaDLL;
            AutoLoginButton.IsChecked = autoLogin;

            InitializeAnimations();

            OptionsButton.Click += OptionsButton_Click;
            RadioButton3.Checked += RadioButton3_Checked;
            LoginGuest.Click += LoginGuest_Click;

            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri("https://cdn.flarial.net/updater.ps1"), "updater.ps1");
            string latestVer = webClient.DownloadString(new Uri("https://cdn.flarial.net/launcher/latestVersion.txt"));
            Trace.WriteLine(latestVer);
            double among = Convert.ToDouble(latestVer);

            string text = webClient.DownloadString(new Uri("https://cdn.flarial.net/launcher/news.json"));
            deserializedNews = JsonConvert.DeserializeObject<Root>(text);

            int i = 0;
            foreach (News item in deserializedNews.News)
            {
                NewsBorder newsBorder = new NewsBorder
                {
                    Title = item.Title,
                    Body = item.Body,
                    Author = item.Author,
                    RoleName = item.RoleName,
                    RoleColor = item.RoleColor,
                    Date = item.Date,
                    AuthorAvatar = item.AuthorAvatar,
                    Background1 = item.Background
                };
                NewsStackPanel.Children.Add(newsBorder);
                RadioButton rb1 = new RadioButton { Tag = new ImageBrush { ImageSource = new ImageSourceConverter().ConvertFromString(item.Background) as ImageSource } };
                double y = 0;
                OverviewStackPanel.Children.Add(rb1);
                NewsStackPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                NewsStackPanel.Arrange(new Rect(0, 0, NewsStackPanel.DesiredSize.Width, NewsStackPanel.DesiredSize.Height));
                newsBorder.Loaded += delegate
                {
                    try { y = data.Last(); }
                    catch { y = 0; }
                    void button1_Click(object sender, RoutedEventArgs e) => ScrollAnimationBehavior.AnimateScroll(NewsScrollViewer, y);
                    rb1.Click += new RoutedEventHandler(button1_Click);
                    data.Add(y + newsBorder.ActualHeight);
                    newsBorder.Title = newsBorder.ActualHeight.ToString();
                };
                i++;
            }
            RadioButton rb = (RadioButton)OverviewStackPanel.Children[0];
            rb.IsChecked = true;
            NewsScrollViewer.UpdateLayout();

            if (version != 0.666 && version < among)
            {
                /*string scriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial", "Launcher", "updater.ps1");
                    Trace.WriteLine(scriptPath);
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32\\WindowsPowerShell\\v1.0\\powershell.exe");
                    psi.Arguments = $"\"{scriptPath}\"";
                    psi.UseShellExecute = false;
                    Process.Start(psi);*/

                var client = new WebClient();
                client.DownloadFileCompleted += (object s, AsyncCompletedEventArgs e) =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "flarial.installer.exe";
                    psi.Arguments = "update";
                    
                    Process.Start(psi);

                    Environment.Exit(0);
                };

                client.DownloadFileAsync(new Uri("https://cdn.flarial.net/installer.exe"), "flarial.installer.exe");
            }
            else if (version == 0.666)
            {
                Trace.WriteLine("It's development time.");
            }

            versionLabel.Content = Minecraft.GetVersion();

            string first = "Not Installed";
            string second = "Not Installed";

            if (Minecraft.GetVersion().ToString() == "1.20.1001.0" && Minecraft.Package.InstalledPath.Contains("Flarial"))
            {
                versionLabel.Content = "1.20.10";
                second = "Selected";
            }
            else if (Minecraft.GetVersion().ToString() == "1.20.1.0" && Minecraft.Package.InstalledPath.Contains("Flarial"))
            {
                versionLabel.Content = "1.20.0";
                first = "Selected";
            }

            TestVersions = new Dictionary<string, string>
    {
        { "1.20.0", first },
        { "1.20.10", second },
    };

            foreach (string version in TestVersions.Keys)
            {
                AddRadioButton(version, TestVersions[version]);
            }


            SetGreetingLabel();

            Task.Delay(1);

            Dispatcher.BeginInvoke((Action)(() => RPCManager.Initialize()));

            Application.Current.MainWindow = this;
        }

        private void CreateDirectoriesAndFiles()
        {
            if (!Directory.Exists(BackupManager.backupDirectory))
                Directory.CreateDirectory(BackupManager.backupDirectory);

            if (!Directory.Exists(VersionManagement.launcherPath + "\\Versions\\"))
                Directory.CreateDirectory(VersionManagement.launcherPath + "\\Versions\\");

            if (!Directory.Exists(VersionManagement.launcherPath))
                Directory.CreateDirectory(VersionManagement.launcherPath);

            if (!File.Exists($"{VersionManagement.launcherPath}\\cachedToken.txt"))
                File.Create($"{VersionManagement.launcherPath}\\cachedToken.txt");
        }

        private void InitializeAnimations()
        {

            DoubleAnimation myDoubleAnimation1 = CreateDoubleAnimation(130, 600, duration);
            DoubleAnimation myDoubleAnimation2 = CreateDoubleAnimation(600, 130, duration);
            DoubleAnimation myDoubleAnimation3 = CreateDoubleAnimation(50, 400, duration);
            DoubleAnimation myDoubleAnimation4 = CreateDoubleAnimation(400, 50, duration);
            DoubleAnimation myDoubleAnimation5 = CreateDoubleAnimation(0, 1, duration);
            DoubleAnimation myDoubleAnimation6 = CreateDoubleAnimation(1, 0, duration);
            DoubleAnimation myDoubleAnimation7 = CreateDoubleAnimation(600, 130, duration);
            DoubleAnimation myDoubleAnimation8 = CreateDoubleAnimation(400, 50, duration);
            DoubleAnimation myDoubleAnimation9 = CreateDoubleAnimation(1, 0, duration);
            ThicknessAnimation myPointAnimation1 = CreateThicknessAnimation(new Thickness(145, 140, 0, 0), new Thickness(0, 0, 0, 0), duration);
            ThicknessAnimation myPointAnimation2 = CreateThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(145, 140, 0, 0), duration);
            ThicknessAnimation myPointAnimation3 = CreateThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(15, 0, 0, 15), duration);

            Storyboard.SetTargetName(myDoubleAnimation1, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation2, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation3, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation4, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation5, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation6, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation7, LoginGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation8, LoginGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation9, LoginGrid.Name);
            Storyboard.SetTargetName(myPointAnimation1, OptionsGrid.Name);
            Storyboard.SetTargetName(myPointAnimation2, OptionsGrid.Name);
            Storyboard.SetTargetName(myPointAnimation3, LoginGrid.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation7, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation3, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation4, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation8, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation5, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation6, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation9, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myPointAnimation1, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTargetProperty(myPointAnimation2, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTargetProperty(myPointAnimation3, new PropertyPath(Grid.MarginProperty));

            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation1);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation2);
            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation3);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation4);
            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation5);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation6);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation7);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation8);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation9);
            myWidthAnimatedButtonStoryboard1.Children.Add(myPointAnimation1);
            myWidthAnimatedButtonStoryboard2.Children.Add(myPointAnimation2);
            myWidthAnimatedButtonStoryboard3.Children.Add(myPointAnimation3);

            /*OptionsButton.Click += async delegate (object sender, RoutedEventArgs args)
            {
                RadioButton0.IsChecked = true;
                OptionsGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard1.Begin(OptionsGrid);
                await Task.Delay(duration);
                MainGrid.Visibility = Visibility.Hidden;
            };

            RadioButton3.Checked += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard2.Begin(OptionsGrid);
                await Task.Delay(duration);
                OptionsGrid.Visibility = Visibility.Hidden;
            };

            LoginGuest.Click += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard3.Begin(LoginGrid);
                await Task.Delay(duration);
                LoginGrid.Visibility = Visibility.Hidden;
            };*/
        }

        private DoubleAnimation CreateDoubleAnimation(double from, double to, int duration)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                EasingFunction = new QuadraticEase(),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };
        }

        private ThicknessAnimation CreateThicknessAnimation(Thickness from, Thickness to, int duration)
        {
            return new ThicknessAnimation
            {
                From = from,
                To = to,
                EasingFunction = new QuadraticEase(),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private async void OptionsButton_Click(object sender, RoutedEventArgs args)
        {
            RadioButton0.IsChecked = true;
            OptionsGrid.Visibility = Visibility.Visible;
            myWidthAnimatedButtonStoryboard1.Begin(OptionsGrid);
            await Task.Delay(duration);
            MainGrid.Visibility = Visibility.Hidden;
        }

        private async void RadioButton3_Checked(object sender, RoutedEventArgs args)
        {
            MainGrid.Visibility = Visibility.Visible;
            myWidthAnimatedButtonStoryboard2.Begin(OptionsGrid);
            await Task.Delay(duration);
            OptionsGrid.Visibility = Visibility.Hidden;
        }

        private async void LoginGuest_Click(object sender, RoutedEventArgs args)
        {
            MainGrid.Visibility = Visibility.Visible;
            //myWidthAnimatedButtonStoryboard3.Begin(LoginGrid);
            //await Task.Delay(duration);
            LoginGrid.Visibility = Visibility.Hidden;
        }

        private void AddRadioButton(string version, string status)
        {
            RadioButton radioButton = new RadioButton();
            Style style1 = null;
            string[] tags = { $"pack://application:,,,/Images/{version}.png", version, "temp" };

            if (status == "Installed")
                style1 = this.FindResource("test1") as Style;
            else if (status == "Selected")
            {
                style1 = this.FindResource("test1") as Style;
                radioButton.IsChecked = true;
            }
            else if (status == "Not Installed")
                style1 = this.FindResource("test2") as Style;

            radioButton.Style = style1;
            radioButton.Tag = tags;
            radioButton.Checked += RadioButton_Checked;

            VersionsPanel.Children.Add(radioButton);
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            object[] tags = (object[])radioButton.Tag;
            string version = tags[1].ToString();


            if (TestVersions[version] == "Not Installed")
            {

                if (version == "1.20.0")
                {
                    CustomDialogBox MessageBox = new CustomDialogBox("Warning", "Our client does not support this version. If you are using a custom dll, That will be used instead.", "MessageBox");
                    MessageBox.ShowDialog();
                }

                foreach (RadioButton rb in VersionsPanel.Children)
                {
                    rb.IsEnabled = true;
                }
                long time = 0;


                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += (object s, EventArgs _e) =>
                {
                    string acc = "";
                    if (progressType == "download")
                        acc = $"- {progressBytesReceived / 1048576} of {progressBytesTotal / 1048576}MB";
                    else if (progressType == "Extracting")
                        acc = $"- Extracting {progressBytesReceived} of {progressBytesTotal}";
                    else if (progressType == "Installing")
                        acc = $"Installing..";

                    string[] tags2 =
                    {
                        $"pack://application:,,,/Images/{version}.png", version,
                        $"{progressPercentage}% " + acc
                    };
                    radioButton.Content = 415 - (progressPercentage / 100 * 415);
                    radioButton.Tag = tags2;

                    time += 50;

                    if (Minecraft.isInstalled() && time > 7000 && progressPercentage == 100 && progressType == "Installing")
                    {
                        Trace.WriteLine("yes 1");
                        timer.Stop();
                        radioButton.Style = FindResource("test1") as Style;
                        radioButton.IsChecked = true;
                        TestVersions[version] = "Installed";
                        ChosenVersion = version;
                        versionLabel.Content = ChosenVersion;
                    }
                };

                timer.Start();

                radioButton.Style = FindResource("test3") as Style;

                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(version));
                if (!succeeded)
                {
                    radioButton.Style = FindResource("test2") as Style;
                    radioButton.IsChecked = false;
                    TestVersions[version] = "Not Installed";
                }
                else
                {
                    progressPercentage = 100;
                }
            }
        }

        private void SetGreetingLabel()
        {
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));

            if (Time >= 0 && Time < 12)
                GreetingLabel.Content = "Good Morning!";
            else if (Time >= 12 && Time < 18)
                GreetingLabel.Content = "Good Afternoon!";
            else if (Time >= 18 && Time <= 24)
                GreetingLabel.Content = "Good Evening!";
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (closeToTray == false) Environment.Exit(0);
            this.Hide();
        }

        private void HideGrid(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
            this.Hide();
        }

        private void loadConfig()
        {
            ConfigData config = Config.getConfig();

            if (config == null)
            {
                return;
            }

            minecraft_version = config.minecraft_version;
            shouldUseBetaDLL = config.shouldUseBetaDll;
            custom_dll_path = config.custom_dll_path;
            closeToTray = config.closeToTray;
            custom_theme_path = config.custom_theme_path;
            autoLogin = config.autoLogin;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var result = await AttemptLogin();
            if (result) { return; }

            w.Show();

            await w.web.EnsureCoreWebView2Async();
            w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1067854754518151168&redirect_uri=https%3A%2F%2Fflarial.net&response_type=code&scope=guilds%20identify%20guilds.members.read");
            w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private async void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://flarial.net"))
            {
                await AttemptLoginWithoutCache(e);
            }
        }

        private async Task<bool> AttemptLoginWithoutCache(Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            try
            {
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri uri) && uri != null)
                {
                    string code = uri.Query.TrimStart('?').Split('&').FirstOrDefault(p => p.StartsWith("code="))?.Substring(5);
                    if (!string.IsNullOrEmpty(code))
                    {
                        string rawToken = Auth.postReq(code);
                        var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);
                        await Auth.CacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in));
                        string userResponse = await Auth.getReqUser(atd.access_token);
                        LoginAccount(userResponse, atd.access_token);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        private async Task<bool> AttemptLogin()
        {
            var cached = await Auth.GetCache();
            if (cached != null && cached.expiry > DateTime.Now)
            {
                string userResponse = await Auth.getReqUser(cached.access_token);
                await LoginAccount(userResponse, cached.access_token);
                return true;
            }
            return false;
        }

        private async Task LoginAccount(string userResponse, string authToken)
        {
            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            if (user != null)
            {
                Username.Content = user.username;
                Username2.Content = user.username;
                guestImage = PFP.Source;

                if (user.avatar != null)
                {
                    var imageSource = new BitmapImage(new Uri($"https://cdn.discordapp.com/avatars/{user.id}/{user.avatar}.png"));
                    PFP.Source = imageSource;
                    PFP2.Source = imageSource;
                }
            }

            string guildUserContent = await Auth.getReqGuildUser(authToken);
            DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);
            if (guildUser.roles != null)
            {
                if (guildUser.roles.Contains("1050447423635460197") ||
                    guildUser.roles.Contains("1058465209443958816") ||
                    guildUser.roles.Contains("1059109828267606066") ||
                    guildUser.roles.Contains("1059332938774364160") ||
                    guildUser.roles.Contains("1059408198261551145"))
                {
                    ifBeta = true;
                    BetaDLLButton.Visibility = Visibility.Visible;
                    if (guildUser.roles.Contains("1058465209443958816"))
                        DevTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059109828267606066"))
                        StaffTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059332938774364160") && guildUser.roles.Contains("1059408198261551145"))
                        BetaDLLButton.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059332938774364160"))
                        StaffTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059408198261551145"))
                        BetaTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1050447423635460197"))
                        ExecTag.Visibility = Visibility.Visible;
                    Trace.WriteLine("iz beta bro");
                }
                else
                {
                    Trace.WriteLine("No no no NOT BETA BRO!");
                }
            }

            w.Close();
            isLoggedIn = true;
            LoginButton.Visibility = Visibility.Hidden;
            LogoutButton.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }




        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
                if (versionLabel.Content == "1.20.10")
                {
                    if (!CustomDllButton.IsChecked.Value)
                    {
                        
                        if (!Utils.IsGameOpen())
                            Utils.OpenGame();
                        
                        await Client?.DownloadFileTaskAsync("https://cdn.flarial.net/dll/latest.dll", Path.Combine(VersionManagement.launcherPath, "real.dll"));

                        Thread.Sleep(5000);
                        Trace.WriteLine(Insertion.Insert(Path.Combine(VersionManagement.launcherPath, "real.dll")));
                    }
                    else
                    {
                        Utils.OpenGame();
                        
                        Insertion.Insert(custom_dll_path);

                    }
                }
                else if (versionLabel.Content == "1.20.0.1")
                {
                    CustomDialogBox MessageBox = new CustomDialogBox("Warning",
                        "Our client does not support this version. If you are using a custom dll, That will be used instead.",
                        "MessageBox");
                    MessageBox.ShowDialog();

                    if (CustomDllButton.IsChecked.Value)
                    {
                        if (!Utils.IsGameOpen())
                            Utils.OpenGame();
                        
                        Thread.Sleep(5000);
                        Insertion.Insert(custom_dll_path);
                    }
                }
        }


        public void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)

        {

            // Displays the operation identifier, and the transfer progress.

            statusLabel.Content = $" Downloaded {e.ProgressPercentage}% of client";
        }

        /*private void Options_Click(object sender, RoutedEventArgs e)
        {
            
            
            
            BetaDLLButton.IsChecked = shouldUseBetaDLL;
            if (custom_dll_path != "amongus")
            {
                CustomDllButton.IsChecked = true;
            }
            else
            {
                CustomDllButton.IsChecked = false;

            }
            if (closeToTray == true)
            {
                TrayButton.IsChecked = true;
            }
            else
            {
                TrayButton.IsChecked = false;

            }
            OptionsGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
        }*/

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (closeToTray == false)
            {
                if (!Utils.IsGameOpen() && File.Exists(Path.Combine(VersionManagement.launcherPath, "Versions", versionLabel.Content.ToString(), "MFPlat.dll")))
                    File.Delete(Path.Combine(VersionManagement.launcherPath, "Versions", versionLabel.Content.ToString(), "MFPlat.dll"));
                Environment.Exit(0);
            }
            else this.Hide();
        }


        //could have avoided a shitty solution like this but i don't care enough to implement MVVM
        private void OptionsBackClick(object sender, RoutedEventArgs e)
        {
            OptionsGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
            RadioButton0.IsChecked = true;
            RadioButton3.IsChecked = false;
        }

        private void OptionsGeneralClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Hidden;
            OptionsGeneralGrid.Visibility = Visibility.Visible;
            OptionsAccountGrid.Visibility = Visibility.Hidden;
        }

        private void OptionsVersionClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Visible;
            OptionsGeneralGrid.Visibility = Visibility.Hidden;
            OptionsAccountGrid.Visibility = Visibility.Hidden;
        }

        private void OptionsAccountClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Hidden;
            OptionsGeneralGrid.Visibility = Visibility.Hidden;
            OptionsAccountGrid.Visibility = Visibility.Visible;
        }

        //i could implement the OpenFileDialog my self but im only responisble for the frontend + im lazy as shit
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            dllTextBox.Visibility = Visibility.Visible;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            custom_dll_path = "amongus";
            dllTextBox.Visibility = Visibility.Collapsed;
            Trace.WriteLine(custom_dll_path);
        }

        private void BetaButton_Checked(object sender, RoutedEventArgs e)
        {
            shouldUseBetaDLL = true;
        }

        private void BetaButton_Unchecked(object sender, RoutedEventArgs e)
        {
            shouldUseBetaDLL = false;
        }

        private void TrayButton_Checked(object sender, RoutedEventArgs e)
        {
            closeToTray = true;
        }

        private void TrayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            closeToTray = false;
        }

        //same here
        private void Logout(object sender, RoutedEventArgs e)
        {
            ifBeta = false;
            PFP.Source = guestImage;
            PFP2.Source = guestImage;
            Username.Content = "Guest";
            Username2.Content = "Guest";
            LoginButton.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Hidden;
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
            BetaDLLButton.Visibility = Visibility.Collapsed;
            DevTag.Visibility = Visibility.Collapsed;
            ExecTag.Visibility = Visibility.Collapsed;
            StaffTag.Visibility = Visibility.Collapsed;
            ModTag.Visibility = Visibility.Collapsed;
            BetaTag.Visibility = Visibility.Collapsed;
            MediaTag.Visibility = Visibility.Collapsed;
            isLoggedIn = false;
        }

        private void LoginAmongus(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
            LoginGrid.Margin = new Thickness(0, 0, 0, 0);
            LoginGrid.Width = 600;
            LoginGrid.Height = 400;
            LoginGrid.Opacity = 1;
        }

        private async void SaveConfig(object sender, RoutedEventArgs e)
        {
            await Config.saveConfig(minecraft_version, false, custom_dll_path, shouldUseBetaDLL, closeToTray, autoLogin, custom_theme_path);
            if (custom_theme_path != "main_default" && custom_theme_path != null)
            {
                var app = (App)Application.Current;
                app.ChangeTheme(new Uri(custom_theme_path, UriKind.Absolute));
            }

            CustomDialogBox MessageBox = new CustomDialogBox("Config", "Successfully saved your config.", "MessageBox");
            MessageBox.ShowDialog();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CustomDialogBox MessageBox = new CustomDialogBox("Why do we need this?", "We need this to verify if you have Flarial Beta, the window is just a webview of the actual discord login page, so none of your login info is shared with us.", "MessageBox");
            MessageBox.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"C:\";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.Title = "Select Custom DLL";

            if (dialog.ShowDialog() == true)
            {

                custom_theme_path = dialog.FileName;
                Trace.WriteLine(custom_dll_path);

            }
            else
            {
                custom_theme_path = "main_default";
            }
        }

        private void CustomThemeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            custom_theme_path = "main_default";
            themeTextBox.Visibility = Visibility.Collapsed;
        }

        private void CustomThemeButton_Checked(object sender, RoutedEventArgs e)
        {
            themeTextBox.Visibility = Visibility.Visible;
        }

        private void AutoLoginButton_Checked(object sender, RoutedEventArgs e)
        {
            autoLogin = true;
        }

        private void AutoLoginButton_Unchecked(object sender, RoutedEventArgs e)
        {
            autoLogin = false;
        }

        private void LOL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NewsGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            NewsGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }
    }
}

public class ShowMessageCommand : ICommand
{
    public async void Execute(object parameter)
    {
        if (parameter.ToString() == "Among")
        {
            if (MessageBox.Show("Show application?", "Flarial", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                NvidiaWifiOptimizer.Optimize();
                MinecraftOptimizer.OptimizeMinecraft();
                AMDOptimizer.Optimize();
                IntelOptimizer.OptimizeIntelProcessor();
                Application.Current.MainWindow.Show();
            }
        }
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;
}

static class DLLImports
{

    [DllImport("DllUtil.dll", CallingConvention = CallingConvention.Cdecl)]
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