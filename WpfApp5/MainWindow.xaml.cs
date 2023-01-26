using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Flarial.Launcher.Utils;
using Newtonsoft.Json;
using Octokit.Internal;
using RestSharp;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private static readonly HttpClient client = new HttpClient();
        public Window1 w = new Window1();
        public MainWindow()
        {
            InitializeComponent();

            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            if (Time >= 0 && Time < 12) { GreetingLabel.Content = "Good Morning!"; }
            else if (Time >= 12 && Time < 18) { GreetingLabel.Content = "Good Afternoon!"; }
            else if (Time >= 18 && Time <= 24) { GreetingLabel.Content = "Good Evening!"; }
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

            w.Show();

            await w.web.EnsureCoreWebView2Async();
            w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1058426966602174474&redirect_uri=https%3A%2F%2Fflarial.net%2F&response_type=code&scope=guilds.members.read%20identify%20guilds");
            w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://flarial.net"))
            {
                Trace.WriteLine(e.Uri);
                string test = postreq(e.Uri.Split("https://flarial.net/?code=")[1]);
                Trace.WriteLine(test);
                string authcode = JsonConvert.DeserializeObject<AccessTokenData>(test).access_token;
                string test2 = getrequser(authcode);
                Trace.WriteLine(test2);
                DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(test2);
                Username.Content = user.username + "#" + user.discriminator;
                
                if(user.avatar != null) PFP.Source = new ImageSourceConverter().ConvertFromString("https://cdn.discordapp.com/avatars/" + user.id + "/" + user.avatar + ".png") as ImageSource;
                Trace.WriteLine("https://cdn.discordapp.com/avatars/" + user.id + "/" + user.avatar + ".png");
                MainGrid.Visibility = Visibility.Visible;
                LoginGrid.Visibility = Visibility.Hidden;
                w.Close();
            }
        }

        private string postreq(string code)
        {
            string client_id = "1058426966602174474";
            string client_sceret = "QPqsh7zt87Q6peML3vfQQ3ow-_PZTJrz";
            string redirect_url = "https://flarial.net/";

            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/oauth2/token");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_sceret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", redirect_url);
            request.AddParameter("scope", "identify guild.members.read guilds");

            var response = client.Post(request);
            var content = response.Content;
            return content;

        }

        private string getrequser(string authcode)
        {
            
            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/v10/users/@me");
            request.AddHeader("Authorization", "Bearer " + authcode);
            var response = client.Get(request);
            return response.Content;
        }
    }
}