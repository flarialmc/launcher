using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octokit;

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
        public MainWindow()
        {
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


            //this is just for testing and a placeholder so feel free to change it to best fit your needs, you'll probably figure it out
            string[] TestVersions = { "1.16.100.4", "1.19.51.1" };
            string ChosenVersion;

            foreach(string version in TestVersions)
            {
                ImageSource[] imagesources = new ImageSource[2] { new BitmapImage(new Uri("/Images/Saul_Goodman.jpg", UriKind.RelativeOrAbsolute)), new BitmapImage(new Uri("/Images/Saul_Goodman2.jpg", UriKind.RelativeOrAbsolute))};
                Style style = this.FindResource("VersionRadioButton") as Style;
                RadioButton radioButton = new RadioButton();
                radioButton.Style = style;
                radioButton.Tag = imagesources;
                radioButton.Checked += RadioButton_Checked;
                VerisonPanel.Children.Add(radioButton);

                async void RadioButton_Checked(object sender, RoutedEventArgs e)
                {
                    ChosenVersion = version;
                    versionLabel.Content = ChosenVersion;

                    try
                    {

                       await VersionManagement.InstallMinecraft(ChosenVersion);
                       
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


            RPCManager.Initialize();


        }

        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e) => this.Close();
        private void HideGrid(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
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
                string rawToken = Auth.postReq(e.Uri.Split("https://flarial.net/?code=")[1]);


                var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);


                authToken = atd.access_token;
                await Auth.cacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in));


                string userResponse = Auth.getReqUser(authToken);



                Trace.WriteLine(userResponse);
                await LoginAccount(userResponse, authToken);
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






            string userResponse = Auth.getReqUser(authToken);



            Trace.WriteLine(userResponse);
            await LoginAccount(userResponse, authToken);
            return true;

        }

        private async Task LoginAccount(string userResponse, string authToken)
        {


            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            if (user != null)
            {
                Username.Content = user.username + "#" + user.discriminator;
                Username2.Content = user.username + "#" + user.discriminator;
                //Auth.putjoinuser(JsonConvert.DeserializeObject<AccessTokenData>(test), user.id);


                if (user.avatar != null) PFP.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

                if (user.avatar != null) PFP2.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

            }

            string guildUserContent = Auth.getReqGuildUser(authToken);
            Trace.WriteLine(guildUserContent);
            
            DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);

            
                if (guildUser.roles.Contains("1059408198261551145"))
                {
                    ifBeta = true;
                    Trace.WriteLine("iz beta bro");
                }
                else
                {
                    Trace.WriteLine("No no no NOT BETA BRO!");
                }
            
            
            

            w.Close();

            LoginGrid.Visibility = Visibility.Hidden;
            await Task.Delay(20);



            MainGrid.Visibility = Visibility.Visible;



            //   BackupManager.createBackup("Safety");
            //  BackupManager.loadBackup("Safety");

        }



        private async void Inject_Click(object sender, RoutedEventArgs e)
        {

            await Injector.Inject("OnixClient.dll", statusLabel);
        }

        private async void Options_Click(object sender, RoutedEventArgs e)
        {
            var L = await
                 VersionManagement.CacheVersionsList();

            var lol = JsonConvert.DeserializeObject<List<VersionManagement.VersionStruct>>(L);

            foreach (var version in lol)
            {

                if (version.Version.StartsWith("1.16.1") || version.Version.StartsWith("1.19.5"))
                {

                }
                //    versionBox.Items.Add(version.Version);
            }

            OptionsGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
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

        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        //same here
        private void Logout(object sender, RoutedEventArgs e)
        {

        }
    }
}