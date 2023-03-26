using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octokit;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;


namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly HttpClient client = new HttpClient();
        public Window1 w = new();
        public bool ifBeta = false;
        public double version = 0.666; // 0.666 will be ignored by the updater, hence it wont update. But for release, it is recommended to use an actual release number.
        public string minecraft_version = "amongus";
        public string custom_dll_path = "amongus";
        public bool closeToTray = false;
        public bool isLoggedIn = false;
        // PLZ REMBER TO CHECK IF USER IS BETA TOO. DONT GO AROUND USING THIS OR ELS PEOPL CAN HAC BETTA DLL!!
        public bool shouldUseBetaDLL = false;
        private ImageSource guestImage;

        public MainWindow()
        {
            
            
            loadConfig();

            if (!Functions.Utils.IsAdministrator)
            {
                MessageBox.Show("Run the application as an Administrator to continue.");
                Process.GetCurrentProcess().Kill();
            }
            Minecraft.Init();

            if (!Directory.Exists(BackupManager.backupDirectory)) { Directory.CreateDirectory(BackupManager.backupDirectory); }
            if (!Directory.Exists(Managers.VersionManagement.launcherPath))
            {
                Directory.CreateDirectory(Managers.VersionManagement.launcherPath);
            }

            if (!File.Exists($"{Managers.VersionManagement.launcherPath}\\cachedToken.txt"))
                File.Create($"{Managers.VersionManagement.launcherPath}\\cachedToken.txt");



            Environment.CurrentDirectory = Managers.VersionManagement.launcherPath;

            InitializeComponent();
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = "set-executionpolicy unrestricted";
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri("https://cdn.flarial.net/updater.ps1"), "updater.ps1");
            string latestVer = webClient.DownloadString(new Uri("https://cdn.flarial.net/launcher/latestVersion.txt"));
            Trace.WriteLine(latestVer);
            double among = Convert.ToDouble(latestVer);
            if (version != 0.666 && version <= among)
            {
                startInfo.Arguments = "updater.ps1";
                process.StartInfo = startInfo;
                process.Start();
                Environment.Exit(0);
            } else if (version == 0.666)
            {
                Trace.WriteLine("It's development time.");
            }

            //this is just for testing and a placeholder so feel free to change it to best fit your needs, you'll probably figure it out
            string[] TestVersions = { "1.16.100.4", "1.19.51.1" };
            string ChosenVersion;

            foreach(string version in TestVersions)
            {
                ImageSource[] imagesources = new ImageSource[2] { new BitmapImage(new Uri($"/Images/{version}.jpg", UriKind.RelativeOrAbsolute)), new BitmapImage(new Uri($"/Images/{version}_2.jpg", UriKind.RelativeOrAbsolute))};
                Style style = this.FindResource("VersionRadioButton") as Style;
                RadioButton radioButton = new RadioButton();
                radioButton.Style = style;
                radioButton.Tag = imagesources;
                if (Minecraft.GetVersion().ToString().StartsWith(version.Remove(5))) radioButton.IsChecked = true;
                radioButton.Checked += RadioButton_Checked;

                VerisonPanel.Children.Add(radioButton);

                async void RadioButton_Checked(object sender, RoutedEventArgs e)
                {
                    ChosenVersion = version;
                    versionLabel.Content = ChosenVersion;
                    
                    try
                    {

                       await Task.Run(() => VersionManagement.InstallMinecraft(ChosenVersion));
                       
                    } catch(RateLimitExceededException)
                    {
                        MessageBox.Show("Octokit Rate Limit was reached.");
                    }
                }
                
                
            }
            

            versionLabel.Content = Minecraft.GetVersion();
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            if (Time >= 0 && Time < 12) { GreetingLabel.Content = "Good Morning!"; }
            else if (Time >= 12 && Time < 18) { GreetingLabel.Content = "Good Afternoon!"; }
            else if (Time >= 18 && Time <= 24) { GreetingLabel.Content = "Good Evening!"; }


            Task.Delay(1);


            Dispatcher.BeginInvoke(() => RPCManager.Initialize());


            Application.Current.MainWindow = this;

        }


        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if(closeToTray == false) Environment.Exit(0);
            this.Hide();
        }
        
        private void HideGrid(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
        }

        private void loadConfig()
        {
            ConfigData? config = Config.getConfig();

            if (config == null)
            {
                return;
            }
            
            minecraft_version = config.minecraft_version;
            shouldUseBetaDLL = config.shouldUseBetaDll;
            custom_dll_path = config.custom_dll_path;
            closeToTray = config.closeToTray;



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

        private async void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
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
                string authToken;
                string sheesh = e.Uri.Split("https://flarial.net/?code=")[1];
                string rawToken = await Task.Run(() => Auth.postReq(sheesh));


                var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);


                authToken = atd.access_token;
                await Task.Run(() =>Auth.cacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in)));


                string userResponse = await Task.Run(() => Auth.getReqUser(authToken));



                Trace.WriteLine(userResponse);
                LoginAccount(userResponse, authToken);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private async Task<bool> AttemptLogin()
        {
            string authToken;


            var Cached = await Auth.getCache();

            if (Cached != null && Cached.expiry > DateTime.Now)
            {
                authToken = Cached.access_token;
                Trace.WriteLine("Cached one is valid.");
            }
            else

            {

                Trace.Write("Invalid cache");
                return false;


            }






            string userResponse = await Task.Run(() => Auth.getReqUser(authToken));



            Trace.WriteLine(userResponse);
            LoginAccount(userResponse, authToken);
            return true;

        }

        private async void LoginAccount(string userResponse, string authToken)
        {


            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            
            if (user == null)
            {
                Username.Content = user.username + "#" + user.discriminator;
                Username2.Content = user.username + "#" + user.discriminator;
                //Auth.putjoinuser(JsonConvert.DeserializeObject<AccessTokenData>(test), user.id);

                guestImage = PFP.Source;

                if (user.avatar != null) PFP.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

                if (user.avatar != null) PFP2.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

            }

            string guildUserContent = await Task.Run(() => Auth.getReqGuildUser(authToken));
            Trace.WriteLine(guildUserContent);
            
            DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);

            
                if (guildUser.roles.Contains("1059408198261551145"))
                {
                    ifBeta = true;
                    BetaDLLButton.Visibility = Visibility.Visible;
                    Trace.WriteLine("iz beta bro");
                }
                else
                {
                    Trace.WriteLine("No no no NOT BETA BRO!");
                }
            
            
            

            w.Close();

            isLoggedIn = true;
            LoginButton.Visibility = Visibility.Hidden;
            LogoutButton.Visibility = Visibility.Visible;

            LoginGrid.Visibility = Visibility.Hidden;



            MainGrid.Visibility = Visibility.Visible;



            //   BackupManager.createBackup("Safety");
            //  BackupManager.loadBackup("Safety");

        }



        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fi = new FileInfo(custom_dll_path);

            if(custom_dll_path == "amongus")
            {
                WebClient webClient = new WebClient();

                DownloadProgressChangedEventHandler among =
                    new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                webClient.DownloadProgressChanged += among;
                await webClient.DownloadFileTaskAsync(new Uri("https://horion.download/dll"), "Among.dll");
                await Injector.Inject($"{Managers.VersionManagement.launcherPath}\\Among.dll", statusLabel);
            }
            else
            {
                if (File.Exists(custom_dll_path) && fi.Extension == ".dll")
                {
                    await Injector.Inject(custom_dll_path, statusLabel);
                }
            }
        }
        
        public void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)

        {

            // Displays the operation identifier, and the transfer progress.

            statusLabel.Content = $" Downloaded {e.ProgressPercentage}% of client";
        }

        private void Options_Click(object sender, RoutedEventArgs e)
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
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if(closeToTray == false) Environment.Exit(0);
            else this.Hide();
        }

        private void versionBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

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
            if(custom_dll_path == "amongus")
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"C:\";
                dialog.DefaultExt = "dll";
                dialog.Filter = "DLL Files|*.dll;";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Multiselect = false;
                dialog.Title = "Select Custom DLL";

                if (dialog.ShowDialog() == true)
                {

                    custom_dll_path = dialog.FileName;
                    Trace.WriteLine(custom_dll_path);

                }
                else
                {
                    dialog.ShowDialog();
                    custom_dll_path = dialog.FileName;
                    Trace.WriteLine(custom_dll_path);
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            custom_dll_path = "amongus";
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
            BetaDLLButton.Visibility = Visibility.Hidden;
            isLoggedIn = false;
        }

        private void LoginAmongus(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
        }

        private async void SaveConfig(object sender, RoutedEventArgs e)
        {

            await Config.saveConfig(minecraft_version, custom_dll_path, shouldUseBetaDLL, closeToTray);

        }

        private void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

    }
}

public class ShowMessageCommand : ICommand
{
    public void Execute(object parameter)
    {
        if (parameter.ToString() == "Among")
        {
            if (MessageBox.Show("Show application?", "Flarial", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
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