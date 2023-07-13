using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.Functions
{
    public static class Injector // yes i stole from my own launcher
    {


        public static async Task Inject(string path, Label Status)
        {
            if (!await Task.Run(() => File.Exists(path)))
            {
                MessageBox.Show("The file does not exist in the provided path.");
                return;
            }

            Utils.OpenGame();
            while (!Utils.IsGameOpen())
            {
                await Task.Delay(1);
            }

            Minecraft.Init();

            Status.Content = "Waiting for Minecraft";


        }

    }
}
