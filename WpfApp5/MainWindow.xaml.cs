using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            Window1 w = new Window1();

            w.Show();

            await w.web.EnsureCoreWebView2Async();
            w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1067854754518151168&redirect_uri=https%3A%2F%2Fflarial.net&response_type=code&scope=guilds%20identify%20guilds.members.read");
            w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://flarial.net"))
            {
                Trace.WriteLine(e.Uri);
            }
        }
    }
}
