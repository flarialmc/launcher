using System.ComponentModel;
using System.Windows;

namespace Flarial.Launcher;
/// <summary>
/// Interaction logic for Window1.xaml
/// </summary>
public partial class Window1 : Window
{
    public string WindowTitle = "https://discord.com/api/oauth2/authorize?client_id=1058426966602174474&response_type=code&redirect_uri=https%3A%2F%2Fflarial.net&scope=guilds.members.read+identify+guilds";
   
    public Window1() => InitializeComponent();

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;  // cancels the window close    
        Hide();      // Programmatically hides the window
    }

}
